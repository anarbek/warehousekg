$token = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method POST -Body '{"userName":"admin","password":"Admin1234!"}' -ContentType "application/json").accessToken
$h = @{Authorization="Bearer $token";"X-Tenant-Id"="ffffffff-ffff-ffff-ffff-ffffffffffff"}
$out = "d:\Projects\warehousekg\fix-results.txt"
"" > $out

# Check pre-order status
$pos = Invoke-RestMethod "http://localhost:5134/api/v1/pre-orders" -Headers $h
$wfPos = $pos | Where-Object { $_.number -like "WF-*" }
"WF Pre-orders: $($wfPos.Count)" >> $out
$wfPos | ForEach-Object { "  $($_.number) status=$($_.status)" >> $out }

# Fix employee linking 
docker exec wkg-postgres psql -U postgres -d WAREHOUSEKG -t -c "UPDATE `"AspNetUsers`" SET `"EmployeeId`" = (SELECT `"Id`" FROM employees WHERE `"Code`"='PRE-001' AND `"TenantId`"='ffffffff-ffff-ffff-ffff-ffffffffffff' LIMIT 1) WHERE `"UserName`"='pre1' AND `"EmployeeId`" IS NULL" 2>>$out
docker exec wkg-postgres psql -U postgres -d WAREHOUSEKG -t -c "UPDATE `"AspNetUsers`" SET `"EmployeeId`" = (SELECT `"Id`" FROM employees WHERE `"Code`"='PRE-002' AND `"TenantId`"='ffffffff-ffff-ffff-ffff-ffffffffffff' LIMIT 1) WHERE `"UserName`"='pre2' AND `"EmployeeId`" IS NULL" 2>>$out
docker exec wkg-postgres psql -U postgres -d WAREHOUSEKG -t -c "UPDATE `"AspNetUsers`" SET `"EmployeeId`" = (SELECT `"Id`" FROM employees WHERE `"Code`"='PRE-003' AND `"TenantId`"='ffffffff-ffff-ffff-ffff-ffffffffffff' LIMIT 1) WHERE `"UserName`"='pre3' AND `"EmployeeId`" IS NULL" 2>>$out
docker exec wkg-postgres psql -U postgres -d WAREHOUSEKG -t -c "UPDATE `"AspNetUsers`" SET `"EmployeeId`" = (SELECT `"Id`" FROM employees WHERE `"Code`"='DRV-001' AND `"TenantId`"='ffffffff-ffff-ffff-ffff-ffffffffffff' LIMIT 1) WHERE `"UserName`"='driverA' AND `"EmployeeId`" IS NULL" 2>>$out
docker exec wkg-postgres psql -U postgres -d WAREHOUSEKG -t -c "UPDATE `"AspNetUsers`" SET `"EmployeeId`" = (SELECT `"Id`" FROM employees WHERE `"Code`"='DRV-002' AND `"TenantId`"='ffffffff-ffff-ffff-ffff-ffffffffffff' LIMIT 1) WHERE `"UserName`"='driverB' AND `"EmployeeId`" IS NULL" 2>>$out
docker exec wkg-postgres psql -U postgres -d WAREHOUSEKG -t -c "UPDATE `"AspNetUsers`" SET `"EmployeeId`" = (SELECT `"Id`" FROM employees WHERE `"Code`"='DRV-003' AND `"TenantId`"='ffffffff-ffff-ffff-ffff-ffffffffffff' LIMIT 1) WHERE `"UserName`"='driverC' AND `"EmployeeId`" IS NULL" 2>>$out
"Linked" >> $out

# Verify employee IDs are set
docker exec wkg-postgres psql -U postgres -d WAREHOUSEKG -t -c "SELECT `"UserName`", `"EmployeeId`" FROM `"AspNetUsers`" WHERE `"UserName`" IN ('pre1','pre2','pre3','driverA','driverB','driverC')" 2>>$out

# Driver employee IDs
$emps = Invoke-RestMethod "http://localhost:5134/api/v1/employees" -Headers $h
$drvA = ($emps | Where-Object { $_.code -eq "DRV-001" }).id
$drvB = ($emps | Where-Object { $_.code -eq "DRV-002" }).id
$drvC = ($emps | Where-Object { $_.code -eq "DRV-003" }).id
"Driver IDs: A=$drvA B=$drvB C=$drvC" >> $out

# Approve and convert remaining pre-orders (those still in Draft)
foreach ($p in $wfPos) {
    $pid = $p.id
    if ($p.status -eq 0) {
        Invoke-RestMethod "http://localhost:5134/api/v1/pre-orders/$pid/submit" -Method POST -Headers $h | Out-Null
        Invoke-RestMethod "http://localhost:5134/api/v1/pre-orders/$pid/approve" -Method POST -Headers $h | Out-Null
        try { 
            $sid = (Invoke-RestMethod "http://localhost:5134/api/v1/pre-orders/$pid/convert" -Method POST -Headers $h).Trim('"')
            "  $($p.number) -> SO $sid" >> $out 
        } catch { "  $($p.number) -> CONVERT FAILED: $_" >> $out }
    } elseif ($p.status -eq 1) {
        Invoke-RestMethod "http://localhost:5134/api/v1/pre-orders/$pid/approve" -Method POST -Headers $h | Out-Null
        try { 
            $sid = (Invoke-RestMethod "http://localhost:5134/api/v1/pre-orders/$pid/convert" -Method POST -Headers $h).Trim('"')
            "  $($p.number) -> SO $sid" >> $out 
        } catch { "  $($p.number) -> CONVERT FAILED: $_" >> $out }
    } elseif ($p.status -eq 4) {
        "  $($p.number) already converted" >> $out
    } else {
        "  $($p.number) status=$($p.status) - skipping" >> $out
    }
}

# Get all converted SOs
$sos = Invoke-RestMethod "http://localhost:5134/api/v1/sales-orders" -Headers $h
$wfSos = $sos | Where-Object { $_.number -like "SO-WF-*" }
"WF Sales Orders: $($wfSos.Count)" >> $out

# Confirm all Draft SOs
foreach ($so in $wfSos) {
    if ($so.status -eq "Draft") {
        Invoke-RestMethod "http://localhost:5134/api/v1/sales-orders/$($so.id)/confirm" -Method POST -Headers $h | Out-Null
        "  Confirmed: $($so.number)" >> $out
    }
}

# Now create routes
$wfSos = Invoke-RestMethod "http://localhost:5134/api/v1/sales-orders" -Headers $h | Where-Object { $_.number -like "SO-WF-*" }
$soIds = $wfSos | ForEach-Object { $_.id }
"SO IDs for routes: $($soIds -join ', ')" >> $out

# Route A: Driver A, 3 SOs (first 3)
if ($soIds.Count -ge 3) {
    $rA = @($soIds[0],$soIds[1],$soIds[2])
    $rBody = "{""code"":""R-WF-A"",""date"":""2026-06-21"",""driverEmployeeId"":""$drvA"",""notes"":""WF Test Route A""}"
    $routeA = (Invoke-RestMethod "http://localhost:5134/api/v1/routes" -Method POST -Body $rBody -ContentType "application/json" -Headers $h).Trim('"')
    "Route A: $routeA" >> $out
    $sn = 1
    foreach ($sid in $rA) {
        $so = Invoke-RestMethod "http://localhost:5134/api/v1/sales-orders/$sid" -Headers $h
        $sBody = "{""customerId"":""$($so.customerId)"",""sequenceNumber"":$sn,""notes"":""Stop $sn""}"
        $stopId = (Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeA/stops" -Method POST -Body $sBody -ContentType "application/json" -Headers $h).Trim('"')
        $shBody = "{""salesOrderId"":""$sid""}"
        Invoke-RestMethod "http://localhost:5134/api/v1/routes/_/stops/$stopId/shipments" -Method POST -Body $shBody -ContentType "application/json" -Headers $h | Out-Null
        "  Stop Stop ${sn} $stopId <- SO $($so.number)" >> $out
        $sn++
    }
}

# Route B: Driver B, 3 SOs
if ($soIds.Count -ge 6) {
    $rB = @($soIds[3],$soIds[4],$soIds[5])
    $rBody = "{""code"":""R-WF-B"",""date"":""2026-06-21"",""driverEmployeeId"":""$drvB"",""notes"":""WF Test Route B""}"
    $routeB = (Invoke-RestMethod "http://localhost:5134/api/v1/routes" -Method POST -Body $rBody -ContentType "application/json" -Headers $h).Trim('"')
    "Route B: $routeB" >> $out
    $sn = 1
    foreach ($sid in $rB) {
        $so = Invoke-RestMethod "http://localhost:5134/api/v1/sales-orders/$sid" -Headers $h
        $sBody = "{""customerId"":""$($so.customerId)"",""sequenceNumber"":$sn,""notes"":""Stop $sn""}"
        $stopId = (Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeB/stops" -Method POST -Body $sBody -ContentType "application/json" -Headers $h).Trim('"')
        $shBody = "{""salesOrderId"":""$sid""}"
        Invoke-RestMethod "http://localhost:5134/api/v1/routes/_/stops/$stopId/shipments" -Method POST -Body $shBody -ContentType "application/json" -Headers $h | Out-Null
        "  Stop Stop ${sn} $stopId <- SO $($so.number)" >> $out
        $sn++
    }
}

# Route C: Driver C, 3 SOs
if ($soIds.Count -ge 9) {
    $rC = @($soIds[6],$soIds[7],$soIds[8])
    $rBody = "{""code"":""R-WF-C"",""date"":""2026-06-21"",""driverEmployeeId"":""$drvC"",""notes"":""WF Test Route C""}"
    $routeC = (Invoke-RestMethod "http://localhost:5134/api/v1/routes" -Method POST -Body $rBody -ContentType "application/json" -Headers $h).Trim('"')
    "Route C: $routeC" >> $out
    $sn = 1
    foreach ($sid in $rC) {
        $so = Invoke-RestMethod "http://localhost:5134/api/v1/sales-orders/$sid" -Headers $h
        $sBody = "{""customerId"":""$($so.customerId)"",""sequenceNumber"":$sn,""notes"":""Stop $sn""}"
        $stopId = (Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeC/stops" -Method POST -Body $sBody -ContentType "application/json" -Headers $h).Trim('"')
        $shBody = "{""salesOrderId"":""$sid""}"
        Invoke-RestMethod "http://localhost:5134/api/v1/routes/_/stops/$stopId/shipments" -Method POST -Body $shBody -ContentType "application/json" -Headers $h | Out-Null
        "  Stop Stop ${sn} $stopId <- SO $($so.number)" >> $out
        $sn++
    }
}

"FIX DONE" >> $out
Get-Content $out
