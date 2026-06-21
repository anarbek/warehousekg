$o="d:\Projects\warehousekg\final-e2e.txt"; "" > $o
function log($m){Write-Host $m;$m >> $o}

$t=(Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method POST -Body '{"userName":"admin","password":"Admin1234!"}' -ContentType "application/json").accessToken
$h=@{Authorization="Bearer $t";"X-Tenant-Id"="ffffffff-ffff-ffff-ffff-ffffffffffff"}

# Get Confirmed WF SOs
$allSo = Invoke-RestMethod "http://localhost:5134/api/v1/sales-orders" -Headers $h
$wfConfirmed = $allSo | Where-Object {$_.number -like "SO-WF-*" -and $_.status -eq "Confirmed"}
$soArr = @($wfConfirmed)
log "Confirmed WF SOs: $($soArr.Count)"

if ($soArr.Count -lt 9) { log "NOT ENOUGH - need 9, have $($soArr.Count)"; exit }

# Get driver employee IDs
$emps = Invoke-RestMethod "http://localhost:5134/api/v1/employees" -Headers $h
$drvA = ($emps | Where-Object {$_.code -eq "DRV-002"}).id
$drvB = ($emps | Where-Object {$_.code -eq "DRV-003"}).id
$drvC = ($emps | Where-Object {$_.code -eq "DRV-002"}).id
log "Using drivers: B=$drvB C=$drvC"

$ts = Get-Date -Format "HHmmss"

# Create 3 fresh routes
$routes = @()
for ($r=0; $r -lt 3; $r++) {
    $code = "R-WF-" + [char](65+$r) + "-" + $ts
    $drv = if ($r -eq 0) {$drvB} elseif ($r -eq 1) {$drvC} else {$drvB}
    $rBody = '{"code":"'+$code+'","date":"2026-06-21T00:00:00Z","driverEmployeeId":"'+$drv+'","notes":"WF Route '+($r+1)+'"}'
    try {
        $rid = (Invoke-RestMethod "http://localhost:5134/api/v1/routes" -Method POST -Body $rBody -ContentType "application/json" -Headers $h).Trim('"')
        $routes += $rid
        log "Route $code : $rid"
    } catch { log "Route $code FAILED: $_"; exit }
}

# Add 3 stops per route + ship shipments
for ($r=0; $r -lt 3; $r++) {
    $rid = $routes[$r]
    $startIdx = $r * 3
    for ($s=0; $s -lt 3; $s++) {
        $soIdx = $startIdx + $s
        $so = $soArr[$soIdx]
        $sn = $s + 1
        
        # Create stop
        $sBody = '{"routeId":"'+$rid+'","sequenceNumber":'+$sn+',"customerId":"'+$so.customerId+'","address":"Address '+$sn+'","notes":"Stop '+$sn+' for '+$so.number+'"}'
        try {
            $stopId = (Invoke-RestMethod "http://localhost:5134/api/v1/routes/$rid/stops" -Method POST -Body $sBody -ContentType "application/json" -Headers $h).Trim('"')
            log "  Stop $sn ($($so.number)): $stopId"
            
            # Assign shipment
            $shBody = '{"salesOrderId":"'+$so.id+'"}'
            Invoke-RestMethod "http://localhost:5134/api/v1/routes/_/stops/$stopId/shipments" -Method POST -Body $shBody -ContentType "application/json" -Headers $h | Out-Null
            log "    Shipment assigned: $($so.number)"
        } catch { log "  Stop $sn FAILED: $_" }
    }
}

# Verify stops exist
foreach ($rid in $routes) {
    $rd = Invoke-RestMethod "http://localhost:5134/api/v1/routes/$rid/detail" -Headers $h
    log "Route $rid : $($rd.stops.Count) stops, status=$($rd.status)"
}

# Now deliver: drivers B and C
log "=== DELIVERY ==="

# Driver B (Petr) delivers Route 0
function deliver($routeId, $user, $pass) {
    $dt = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method POST -Body "{`"userName`":`"$user`",`"password`":`"$pass`"}" -ContentType "application/json").accessToken
    $dh = @{Authorization="Bearer $dt";"X-Tenant-Id"="ffffffff-ffff-ffff-ffff-ffffffffffff"}
    
    Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeId/start" -Method POST -Headers $dh | Out-Null
    $rd = Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeId/detail" -Headers $h
    log "  Started, stops: $($rd.stops.Count)"
    
    foreach ($stop in $rd.stops) {
        Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeId/stops/$($stop.id)/arrive" -Method POST -Headers $dh | Out-Null
        Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeId/stops/$($stop.id)/complete" -Method POST -Headers $dh | Out-Null
        log "    Stop $($stop.sequenceNumber) ($($stop.customerName)) delivered"
    }
    
    Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeId/complete" -Method POST -Headers $dh | Out-Null
    log "  Route completed"
}

# Use drivers B and C (A has bad employee ID)
deliver $routes[0] "driverB" "Drv1234!"
deliver $routes[1] "driverC" "Drv1234!"
deliver $routes[2] "driverB" "Drv1234!"

# Verify
log "=== VERIFICATION ==="
$finalSo = Invoke-RestMethod "http://localhost:5134/api/v1/sales-orders" -Headers $h | Where-Object {$_.number -like "SO-WF-*"}
$shipped = ($finalSo | Where-Object {$_.status -eq "Shipped"}).Count
log "WF SOs: $($finalSo.Count), Shipped: $shipped"

$items = Invoke-RestMethod "http://localhost:5134/api/v1/inventory-items" -Headers $h
log "Stock: Bread=$($items[0].quantityOnHand) Milk=$($items[1].quantityOnHand) Cheese=$($items[2].quantityOnHand) Ayran=$($items[3].quantityOnHand)"

log "DONE"
