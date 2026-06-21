$t=(Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method POST -Body '{"userName":"admin","password":"Admin1234!"}' -ContentType "application/json").accessToken
$h=@{Authorization="Bearer $t";"X-Tenant-Id"="ffffffff-ffff-ffff-ffff-ffffffffffff"}
$o="d:\Projects\warehousekg\routes-out.txt"; "" > $o

# Fix DRV-001 employee
try { Invoke-RestMethod "http://localhost:5134/api/v1/employees/e1000000-0000-0000-0000-000000000001" -Method DELETE -Headers $h } catch {}
$newDrv = (Invoke-RestMethod "http://localhost:5134/api/v1/employees" -Method POST -Body '{"code":"DRV-001","firstName":"Ivan","lastName":"Driverov","isActive":true}' -ContentType "application/json" -Headers $h).Trim('"')
"DRV-001: $newDrv" >> $o

# Get valid driver IDs
$emps = Invoke-RestMethod "http://localhost:5134/api/v1/employees" -Headers $h
$drvA = ($emps | Where-Object {$_.code -eq "DRV-001"}).id
$drvB = ($emps | Where-Object {$_.code -eq "DRV-002"}).id
$drvC = ($emps | Where-Object {$_.code -eq "DRV-003"}).id
"Drivers: A=$drvA B=$drvB C=$drvC" >> $o

# Get Confirmed WF SOs
$wfSos = Invoke-RestMethod "http://localhost:5134/api/v1/sales-orders" -Headers $h | Where-Object {$_.number -like "SO-WF-*" -and $_.status -eq "Confirmed"}
$soArr = @($wfSos)
"Confirmed SOs: $($soArr.Count)" >> $o

# Create Route A
$rBodyA = '{"code":"R-WF-A2","date":"2026-06-21T00:00:00Z","driverEmployeeId":"'+$drvA+'","notes":"Route A Ivan"}'
$routeA = (Invoke-RestMethod "http://localhost:5134/api/v1/routes" -Method POST -Body $rBodyA -ContentType "application/json" -Headers $h).Trim('"')
"Route A: $routeA" >> $o
if ($soArr.Count -ge 3) {
    $sn=1; foreach ($so in $soArr[0..2]) {
        $sB = '{"customerId":"'+$so.customerId+'","sequenceNumber":'+$sn+',"notes":"Stop '+$sn+'"}'
        $stopId = (Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeA/stops" -Method POST -Body $sB -ContentType "application/json" -Headers $h).Trim('"')
        $shB = '{"salesOrderId":"'+$so.id+'"}'
        Invoke-RestMethod "http://localhost:5134/api/v1/routes/_/stops/$stopId/shipments" -Method POST -Body $shB -ContentType "application/json" -Headers $h | Out-Null
        "  Stop Stop ${sn} $stopId <- $($so.number)" >> $o
        $sn++
    }
}

# Route B
$rBodyB = '{"code":"R-WF-B2","date":"2026-06-21T00:00:00Z","driverEmployeeId":"'+$drvB+'","notes":"Route B Petr"}'
$routeB = (Invoke-RestMethod "http://localhost:5134/api/v1/routes" -Method POST -Body $rBodyB -ContentType "application/json" -Headers $h).Trim('"')
"Route B: $routeB" >> $o
if ($soArr.Count -ge 6) {
    $sn=1; foreach ($so in $soArr[3..5]) {
        $sB = '{"customerId":"'+$so.customerId+'","sequenceNumber":'+$sn+',"notes":"Stop '+$sn+'"}'
        $stopId = (Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeB/stops" -Method POST -Body $sB -ContentType "application/json" -Headers $h).Trim('"')
        $shB = '{"salesOrderId":"'+$so.id+'"}'
        Invoke-RestMethod "http://localhost:5134/api/v1/routes/_/stops/$stopId/shipments" -Method POST -Body $shB -ContentType "application/json" -Headers $h | Out-Null
        "  Stop Stop ${sn} $stopId <- $($so.number)" >> $o
        $sn++
    }
}

# Route C
$rBodyC = '{"code":"R-WF-C2","date":"2026-06-21T00:00:00Z","driverEmployeeId":"'+$drvC+'","notes":"Route C Sergey"}'
$routeC = (Invoke-RestMethod "http://localhost:5134/api/v1/routes" -Method POST -Body $rBodyC -ContentType "application/json" -Headers $h).Trim('"')
"Route C: $routeC" >> $o
if ($soArr.Count -ge 9) {
    $sn=1; foreach ($so in $soArr[6..8]) {
        $sB = '{"customerId":"'+$so.customerId+'","sequenceNumber":'+$sn+',"notes":"Stop '+$sn+'"}'
        $stopId = (Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeC/stops" -Method POST -Body $sB -ContentType "application/json" -Headers $h).Trim('"')
        $shB = '{"salesOrderId":"'+$so.id+'"}'
        Invoke-RestMethod "http://localhost:5134/api/v1/routes/_/stops/$stopId/shipments" -Method POST -Body $shB -ContentType "application/json" -Headers $h | Out-Null
        "  Stop Stop ${sn} $stopId <- $($so.number)" >> $o
        $sn++
    }
}

"DONE" >> $o
Get-Content $o
