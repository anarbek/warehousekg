$out = "d:\Projects\warehousekg\e2e-final.txt"
"" > $out
function log($m) { Write-Host $m; $m >> $out }

# ==== AUTH ====
$t = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method POST -Body '{"userName":"admin","password":"Admin1234!"}' -ContentType "application/json").accessToken
$h = @{Authorization="Bearer $t";"X-Tenant-Id"="ffffffff-ffff-ffff-ffff-ffffffffffff"}

# ==== Check current state ====
$pos = Invoke-RestMethod "http://localhost:5134/api/v1/pre-orders" -Headers $h
$wfPos = $pos | Where-Object { $_.number -like "WF-*" }
log "WF Pre-orders found: $($wfPos.Count)"

# ==== Approve + Convert each Draft PO ====
foreach ($p in $wfPos) {
    if ($p.status -ne 0) { continue }
    $poId = $p.id
    try {
        Invoke-RestMethod "http://localhost:5134/api/v1/pre-orders/$poId/submit" -Method POST -Headers $h | Out-Null
        Invoke-RestMethod "http://localhost:5134/api/v1/pre-orders/$poId/approve" -Method POST -Headers $h | Out-Null
        $sid = (Invoke-RestMethod "http://localhost:5134/api/v1/pre-orders/$poId/convert" -Method POST -Headers $h).Trim('"')
        log "  $($p.number) -> SO $sid"
    } catch {
        $err = $_.Exception.Message
        if ($err -match "409") { log "  $($p.number): already processed" }
        else { log "  $($p.number): ERROR $err" }
    }
}

# ==== Confirm all WF Sales Orders ====
$sos = Invoke-RestMethod "http://localhost:5134/api/v1/sales-orders" -Headers $h
$wfSos = $sos | Where-Object { $_.number -like "SO-WF-*" }
log "WF Sales Orders: $($wfSos.Count)"

foreach ($so in $wfSos) {
    if ($so.status -eq "Draft") {
        Invoke-RestMethod "http://localhost:5134/api/v1/sales-orders/$($so.id)/confirm" -Method POST -Headers $h | Out-Null
        log "  Confirmed: $($so.number)"
    } else {
        log "  $($so.number): $($so.status)"
    }
}

# ==== Get Driver Employee IDs ====
$emps = Invoke-RestMethod "http://localhost:5134/api/v1/employees" -Headers $h
$drvA = ($emps | Where-Object { $_.code -eq "DRV-001" }).id
$drvB = ($emps | Where-Object { $_.code -eq "DRV-002" }).id
$drvC = ($emps | Where-Object { $_.code -eq "DRV-003" }).id
log "Drivers: A=$drvA B=$drvB C=$drvC"

# ==== Create Routes with Stops + Shipments ====
$wfSos = Invoke-RestMethod "http://localhost:5134/api/v1/sales-orders" -Headers $h | Where-Object { $_.number -like "SO-WF-*" -and $_.status -eq "Confirmed" }
$soArr = @($wfSos)
log "Confirmed WF SOs for routing: $($soArr.Count)"

if ($soArr.Count -ge 9) {
    # Route A: Driver A gets SOs 0,1,2
    $rA = $soArr[0..2]
    $rBody = "{`"code`":`"R-WF-A`",`"date`":`"2026-06-21`",`"driverEmployeeId`":`"$drvA`",`"notes`":`"Route A - Ivan`"}"
    $routeA = (Invoke-RestMethod "http://localhost:5134/api/v1/routes" -Method POST -Body $rBody -ContentType "application/json" -Headers $h).Trim('"')
    log "Route A: $routeA"
    $sn = 1
    foreach ($so in $rA) {
        $sBody = "{`"customerId`":`"$($so.customerId)`",`"sequenceNumber`":$sn,`"notes`":`"Stop $sn`"}"
        $stopId = (Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeA/stops" -Method POST -Body $sBody -ContentType "application/json" -Headers $h).Trim('"')
        $shBody = "{`"salesOrderId`":`"$($so.id)`"}"
        Invoke-RestMethod "http://localhost:5134/api/v1/routes/_/stops/$stopId/shipments" -Method POST -Body $shBody -ContentType "application/json" -Headers $h | Out-Null
        log "  Stop $sn <- $($so.number) ($($so.customerName))"
        $sn++
    }

    # Route B: Driver B gets SOs 3,4,5
    $rB = $soArr[3..5]
    $rBody = "{`"code`":`"R-WF-B`",`"date`":`"2026-06-21`",`"driverEmployeeId`":`"$drvB`",`"notes`":`"Route B - Petr`"}"
    $routeB = (Invoke-RestMethod "http://localhost:5134/api/v1/routes" -Method POST -Body $rBody -ContentType "application/json" -Headers $h).Trim('"')
    log "Route B: $routeB"
    $sn = 1
    foreach ($so in $rB) {
        $sBody = "{`"customerId`":`"$($so.customerId)`",`"sequenceNumber`":$sn,`"notes`":`"Stop $sn`"}"
        $stopId = (Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeB/stops" -Method POST -Body $sBody -ContentType "application/json" -Headers $h).Trim('"')
        $shBody = "{`"salesOrderId`":`"$($so.id)`"}"
        Invoke-RestMethod "http://localhost:5134/api/v1/routes/_/stops/$stopId/shipments" -Method POST -Body $shBody -ContentType "application/json" -Headers $h | Out-Null
        log "  Stop $sn <- $($so.number) ($($so.customerName))"
        $sn++
    }

    # Route C: Driver C gets SOs 6,7,8
    $rC = $soArr[6..8]
    $rBody = "{`"code`":`"R-WF-C`",`"date`":`"2026-06-21`",`"driverEmployeeId`":`"$drvC`",`"notes`":`"Route C - Sergey`"}"
    $routeC = (Invoke-RestMethod "http://localhost:5134/api/v1/routes" -Method POST -Body $rBody -ContentType "application/json" -Headers $h).Trim('"')
    log "Route C: $routeC"
    $sn = 1
    foreach ($so in $rC) {
        $sBody = "{`"customerId`":`"$($so.customerId)`",`"sequenceNumber`":$sn,`"notes`":`"Stop $sn`"}"
        $stopId = (Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeC/stops" -Method POST -Body $sBody -ContentType "application/json" -Headers $h).Trim('"')
        $shBody = "{`"salesOrderId`":`"$($so.id)`"}"
        Invoke-RestMethod "http://localhost:5134/api/v1/routes/_/stops/$stopId/shipments" -Method POST -Body $shBody -ContentType "application/json" -Headers $h | Out-Null
        log "  Stop $sn <- $($so.number) ($($so.customerName))"
        $sn++
    }

    # ==== DELIVERY: Each driver delivers their route ====
    log "=== DELIVERY ==="
    
    function driverDeliver($routeId, $user, $pass) {
        $dt = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method POST -Body "{`"userName`":`"$user`",`"password`":`"$pass`"}" -ContentType "application/json").accessToken
        $dh = @{Authorization="Bearer $dt";"X-Tenant-Id"="ffffffff-ffff-ffff-ffff-ffffffffffff"}
        
        Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeId/start" -Method POST -Headers $dh | Out-Null
        $rd = Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeId/detail" -Headers $h
        log "Route $routeId started, $($rd.stops.Count) stops"
        
        foreach ($stop in $rd.stops) {
            Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeId/stops/$($stop.id)/arrive" -Method POST -Headers $dh | Out-Null
            Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeId/stops/$($stop.id)/complete" -Method POST -Headers $dh | Out-Null
            log "  Stop $($stop.sequenceNumber) completed: $($stop.customerName)"
        }
        
        Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeId/complete" -Method POST -Headers $dh | Out-Null
        log "Route $routeId completed"
    }
    
    driverDeliver $routeA "driverA" "Drv1234!"
    driverDeliver $routeB "driverB" "Drv1234!"
    driverDeliver $routeC "driverC" "Drv1234!"
} else {
    log "NOT ENOUGH SOs: $($soArr.Count)"
}

# ==== VERIFY ====
log "=== VERIFICATION ==="
$finalSos = Invoke-RestMethod "http://localhost:5134/api/v1/sales-orders" -Headers $h | Where-Object { $_.number -like "SO-WF-*" }
$shipped = ($finalSos | Where-Object { $_.status -eq "Shipped" }).Count
log "WF SOs: $($finalSos.Count) total, $shipped Shipped"

# Check final inventory
$items = Invoke-RestMethod "http://localhost:5134/api/v1/inventory-items" -Headers $h
$iBread = "a4396dc7-ca44-4c7a-ac7f-ef67297fee9b"
$iMilk = "b90df7e4-510c-4aa8-a1c1-8e42a448f7e3"
$iCheese = "0f578667-e166-4fbc-b407-80f8ddd30e3f"
$b = ($items | Where-Object { $_.id -eq $iBread }).quantityOnHand
$m = ($items | Where-Object { $_.id -eq $iMilk }).quantityOnHand
$c = ($items | Where-Object { $_.id -eq $iCheese }).quantityOnHand
log "Stock: Bread=$b Milk=$m Cheese=$c"

# Check routes
$routes = Invoke-RestMethod "http://localhost:5134/api/v1/routes" -Headers $h | Where-Object { $_.code -like "R-WF-*" }
log "Routes: $($routes.Count)"
foreach ($r in $routes) { log "  $($r.code): status=$($r.status) stops=$($r.stopCount)" }

log "=== E2E COMPLETE ==="
