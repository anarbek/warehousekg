$o="d:\Projects\warehousekg\delivery-out.txt"; "" > $o

function login($u,$p) {
    $t=(Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method POST -Body "{`"userName`":`"$u`",`"password`":`"$p`"}" -ContentType "application/json").accessToken
    return @{Authorization="Bearer $t";"X-Tenant-Id"="ffffffff-ffff-ffff-ffff-ffffffffffff"}
}

$adminH = login "admin" "Admin1234!"

# Route info
$routeA="930e2bae-2ba6-4ed2-8fea-f9b0f1234a77"
$routeB="ee50cf79-0e1e-485e-a23f-6a1350f843d2"
$routeC="b0e80a19-d29d-42ac-ba36-9539d56cc04b"

# Driver A (Ivan) delivers Route A
"=== Driver A (Ivan) ===" >> $o
$drvAH = login "driverA" "Drv1234!"
try {
    Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeA/start" -Method POST -Headers $drvAH | Out-Null
    $rd = Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeA/detail" -Headers $adminH
    "Started, stops: $($rd.stops.Count)" >> $o
    foreach ($stop in $rd.stops) {
        Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeA/stops/$($stop.id)/arrive" -Method POST -Headers $drvAH | Out-Null
        Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeA/stops/$($stop.id)/complete" -Method POST -Headers $drvAH | Out-Null
        "  Stop $($stop.sequenceNumber): $($stop.customerName) DONE" >> $o
    }
    Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeA/complete" -Method POST -Headers $drvAH | Out-Null
    "Route A COMPLETED" >> $o
} catch { "Route A ERROR: $_" >> $o }

# Driver B (Petr) delivers Route B
"=== Driver B (Petr) ===" >> $o
$drvBH = login "driverB" "Drv1234!"
try {
    Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeB/start" -Method POST -Headers $drvBH | Out-Null
    $rd = Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeB/detail" -Headers $adminH
    "Started, stops: $($rd.stops.Count)" >> $o
    foreach ($stop in $rd.stops) {
        Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeB/stops/$($stop.id)/arrive" -Method POST -Headers $drvBH | Out-Null
        Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeB/stops/$($stop.id)/complete" -Method POST -Headers $drvBH | Out-Null
        "  Stop $($stop.sequenceNumber): $($stop.customerName) DONE" >> $o
    }
    Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeB/complete" -Method POST -Headers $drvBH | Out-Null
    "Route B COMPLETED" >> $o
} catch { "Route B ERROR: $_" >> $o }

# Driver C (Sergey) delivers Route C
"=== Driver C (Sergey) ===" >> $o
$drvCH = login "driverC" "Drv1234!"
try {
    Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeC/start" -Method POST -Headers $drvCH | Out-Null
    $rd = Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeC/detail" -Headers $adminH
    "Started, stops: $($rd.stops.Count)" >> $o
    foreach ($stop in $rd.stops) {
        Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeC/stops/$($stop.id)/arrive" -Method POST -Headers $drvCH | Out-Null
        Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeC/stops/$($stop.id)/complete" -Method POST -Headers $drvCH | Out-Null
        "  Stop $($stop.sequenceNumber): $($stop.customerName) DONE" >> $o
    }
    Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeC/complete" -Method POST -Headers $drvCH | Out-Null
    "Route C COMPLETED" >> $o
} catch { "Route C ERROR: $_" >> $o }

# Verify
"=== VERIFY ===" >> $o
$wfSos = Invoke-RestMethod "http://localhost:5134/api/v1/sales-orders" -Headers $adminH | Where-Object {$_.number -like "SO-WF-*"}
$shipped = ($wfSos | Where-Object {$_.status -eq "Shipped"}).Count
"WF SOs: $($wfSos.Count) total, $shipped Shipped" >> $o

# Routes
$routes = Invoke-RestMethod "http://localhost:5134/api/v1/routes" -Headers $adminH | Where-Object {$_.code -like "R-WF-*"}
foreach ($r in $routes) { "Route $($r.code): status=$($r.status) stops=$($r.stopCount)" >> $o }

# Stock
$items = Invoke-RestMethod "http://localhost:5134/api/v1/inventory-items" -Headers $adminH
$ids = @{
    Bread="a4396dc7-ca44-4c7a-ac7f-ef67297fee9b"
    Milk="b90df7e4-510c-4aa8-a1c1-8e42a448f7e3"
    Cheese="0f578667-e166-4fbc-b407-80f8ddd30e3f"
    Ayran="21ca97d7-53f7-4ccd-a98a-3af249e118c0"
    Water="a281f05c-331c-4bbe-8486-e1fb03c7cb01"
}
foreach ($k in $ids.Keys) {
    $v = ($items | Where-Object {$_.id -eq $ids[$k]}).quantityOnHand
    "$k=$v" >> $o
}

"DONE" >> $o
Get-Content $o
