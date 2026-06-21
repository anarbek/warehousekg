$ErrorActionPreference = "Continue"
$out = "d:\Projects\warehousekg\full-workflow-result.txt"
"" > $out

function log($msg) { Write-Host $msg; $msg >> $out }

# ========== AUTH ==========
$token = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method POST -Body '{"userName":"admin","password":"Admin1234!"}' -ContentType "application/json").accessToken
$adminH = @{Authorization="Bearer $token";"X-Tenant-Id"="ffffffff-ffff-ffff-ffff-ffffffffffff"}

# Reference IDs
$custE2E = "5ed63d5e-6cc9-4611-85ed-c0c885320429"
$custFaiza = "c6fb2277-eafa-497d-a168-75e90a13121b"
$custSam = "51c8406c-8c6c-46c5-a87e-8c18c20d7885"
$whDepo1 = "1ac52ff4-ac6c-486d-8666-f09308b3d0af"
$iBread = "a4396dc7-ca44-4c7a-ac7f-ef67297fee9b"
$iMilk = "b90df7e4-510c-4aa8-a1c1-8e42a448f7e3"
$iCheese = "0f578667-e166-4fbc-b407-80f8ddd30e3f"
$iAyran = "21ca97d7-53f7-4ccd-a98a-3af249e118c0"
$iWater = "a281f05c-331c-4bbe-8486-e1fb03c7cb01"

# ========== STEP 1: Create Preseller Users + Employees ==========
log "=== STEP 1: Creating 3 Presellers ==="
$presellers = @()
$preData = @(
    @{user="pre1"; pass="Pre1234!"; code="PRE-001"; first="Alice"; last="Preseller"},
    @{user="pre2"; pass="Pre1234!"; code="PRE-002"; first="Bob"; last="Preseller"},
    @{user="pre3"; pass="Pre1234!"; code="PRE-003"; first="Carol"; last="Preseller"}
)
foreach ($pd in $preData) {
    try {
        $uid = (Invoke-RestMethod "http://localhost:5134/api/v1/users" -Method POST -Body "{""userName"":""$($pd.user)"",""email"":""$($pd.user)@test.local"",""password"":""$($pd.pass)"",""roles"":[""Preseller""]}" -ContentType "application/json" -Headers $adminH).Trim('"')
        log "  User $($pd.user): $uid"
    } catch { log "  User $($pd.user): exists" }
    
    try {
        $eid = (Invoke-RestMethod "http://localhost:5134/api/v1/employees" -Method POST -Body "{""code"":""$($pd.code)"",""firstName"":""$($pd.first)"",""lastName"":""$($pd.last)"",""isActive"":true}" -ContentType "application/json" -Headers $adminH).Trim('"')
        log "  Employee $($pd.code): $eid"
    } catch { log "  Employee $($pd.code): exists" }
}

# Get actual preseller employee IDs
$emps = Invoke-RestMethod "http://localhost:5134/api/v1/employees" -Headers $adminH
$pre1EmpId = ($emps | Where-Object { $_.code -eq "PRE-001" }).id
$pre2EmpId = ($emps | Where-Object { $_.code -eq "PRE-002" }).id
$pre3EmpId = ($emps | Where-Object { $_.code -eq "PRE-003" }).id
log "Preseller Emp IDs: $pre1EmpId, $pre2EmpId, $pre3EmpId"

# ========== STEP 2: Create Driver Users + Employees ==========
log "=== STEP 2: Creating 3 Drivers ==="
$drivers = @()
$drvData = @(
    @{user="driverA"; pass="Drv1234!"; code="DRV-001"; first="Ivan"; last="Driverov"},
    @{user="driverB"; pass="Drv1234!"; code="DRV-002"; first="Petr"; last="Voditel"},
    @{user="driverC"; pass="Drv1234!"; code="DRV-003"; first="Sergey"; last="Rulkin"}
)
foreach ($dd in $drvData) {
    try {
        $uid = (Invoke-RestMethod "http://localhost:5134/api/v1/users" -Method POST -Body "{""userName"":""$($dd.user)"",""email"":""$($dd.user)@test.local"",""password"":""$($dd.pass)"",""roles"":[""Driver""]}" -ContentType "application/json" -Headers $adminH).Trim('"')
        log "  User $($dd.user): $uid"
    } catch { log "  User $($dd.user): exists" }
    
    try {
        $eid = (Invoke-RestMethod "http://localhost:5134/api/v1/employees" -Method POST -Body "{""code"":""$($dd.code)"",""firstName"":""$($dd.first)"",""lastName"":""$($dd.last)"",""isActive"":true}" -ContentType "application/json" -Headers $adminH).Trim('"')
        log "  Employee $($dd.code): $eid"
    } catch { log "  Employee $($dd.code): exists" }
}

# Link employees to users via DB (since API doesn't support this directly)
log "Linking employees to users..."
docker exec wkg-postgres psql -U postgres -d WAREHOUSEKG -c "UPDATE ""AspNetUsers"" SET ""EmployeeId"" = (SELECT ""Id"" FROM employees WHERE ""Code""='PRE-001' AND ""TenantId""='ffffffff-ffff-ffff-ffff-ffffffffffff') WHERE ""UserName""='pre1'" 2>> $out
docker exec wkg-postgres psql -U postgres -d WAREHOUSEKG -c "UPDATE ""AspNetUsers"" SET ""EmployeeId"" = (SELECT ""Id"" FROM employees WHERE ""Code""='PRE-002' AND ""TenantId""='ffffffff-ffff-ffff-ffff-ffffffffffff') WHERE ""UserName""='pre2'" 2>> $out
docker exec wkg-postgres psql -U postgres -d WAREHOUSEKG -c "UPDATE ""AspNetUsers"" SET ""EmployeeId"" = (SELECT ""Id"" FROM employees WHERE ""Code""='PRE-003' AND ""TenantId""='ffffffff-ffff-ffff-ffff-ffffffffffff') WHERE ""UserName""='pre3'" 2>> $out
docker exec wkg-postgres psql -U postgres -d WAREHOUSEKG -c "UPDATE ""AspNetUsers"" SET ""EmployeeId"" = (SELECT ""Id"" FROM employees WHERE ""Code""='DRV-001' AND ""TenantId""='ffffffff-ffff-ffff-ffff-ffffffffffff') WHERE ""UserName""='driverA'" 2>> $out
docker exec wkg-postgres psql -U postgres -d WAREHOUSEKG -c "UPDATE ""AspNetUsers"" SET ""EmployeeId"" = (SELECT ""Id"" FROM employees WHERE ""Code""='DRV-002' AND ""TenantId""='ffffffff-ffff-ffff-ffff-ffffffffffff') WHERE ""UserName""='driverB'" 2>> $out
docker exec wkg-postgres psql -U postgres -d WAREHOUSEKG -c "UPDATE ""AspNetUsers"" SET ""EmployeeId"" = (SELECT ""Id"" FROM employees WHERE ""Code""='DRV-003' AND ""TenantId""='ffffffff-ffff-ffff-ffff-ffffffffffff') WHERE ""UserName""='driverC'" 2>> $out
log "Employees linked to users"

# ========== STEP 3: Presellers create pre-orders ==========
log "=== STEP 3: Presellers Creating Pre-Orders ==="
$allPoIds = @()

function LoginAs($user, $pass) {
    $t = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method POST -Body "{""userName"":""$user"",""password"":""$pass""}" -ContentType "application/json").accessToken
    return @{Authorization="Bearer $t";"X-Tenant-Id"="ffffffff-ffff-ffff-ffff-ffffffffffff"}
}

function CreatePO($headers, $number, $custId, $paymentType, $items) {
    $lines = "["
    $first = $true
    foreach ($it in $items) {
        if (-not $first) { $lines += "," }
        $lines += "{""inventoryItemId"":""$($it.id)"",""quantity"":$($it.qty),""unitPrice"":$($it.price),""discountPercent"":$($it.disc)}"
        $first = $false
    }
    $lines += "]"
    $body = "{""number"":""$number"",""customerId"":""$custId"",""warehouseId"":""$whDepo1"",""paymentType"":""$paymentType"",""currency"":""KGS"",""lines"":$lines}"
    $id = (Invoke-RestMethod "http://localhost:5134/api/v1/pre-orders" -Method POST -Body $body -ContentType "application/json" -Headers $headers).Trim('"')
    return $id
}

# Preseller 1 (Alice): 3 orders for E2E Customer
$pre1H = LoginAs "pre1" "Pre1234!"
$po1 = CreatePO $pre1H "WF-PRE1-001" $custE2E "Cash" @(@{id=$iBread;qty=3;price=100;disc=0}, @{id=$iMilk;qty=5;price=85;disc=5})
$po2 = CreatePO $pre1H "WF-PRE1-002" $custFaiza "Card" @(@{id=$iCheese;qty=4;price=200;disc=0}, @{id=$iAyran;qty=10;price=60;disc=0})
$po3 = CreatePO $pre1H "WF-PRE1-003" $custSam "Bank Transfer" @(@{id=$iWater;qty=6;price=55;disc=0})
log "Pre1: $po1, $po2, $po3"

# Preseller 2 (Bob): 3 orders
$pre2H = LoginAs "pre2" "Pre1234!"
$po4 = CreatePO $pre2H "WF-PRE2-001" $custE2E "Cash" @(@{id=$iBread;qty=2;price=100;disc=0}, @{id=$iMilk;qty=4;price=85;disc=0}, @{id=$iWater;qty=3;price=55;disc=0})
$po5 = CreatePO $pre2H "WF-PRE2-002" $custFaiza "Card" @(@{id=$iCheese;qty=3;price=210;disc=0}, @{id=$iAyran;qty=8;price=60;disc=5})
$po6 = CreatePO $pre2H "WF-PRE2-003" $custSam "Cash" @(@{id=$iBread;qty=4;price=95;disc=0})
log "Pre2: $po4, $po5, $po6"

# Preseller 3 (Carol): 3 orders
$pre3H = LoginAs "pre3" "Pre1234!"
$po7 = CreatePO $pre3H "WF-PRE3-001" $custE2E "Card" @(@{id=$iMilk;qty=6;price=85;disc=0}, @{id=$iCheese;qty=2;price=195;disc=0})
$po8 = CreatePO $pre3H "WF-PRE3-002" $custFaiza "Bank Transfer" @(@{id=$iBread;qty=3;price=100;disc=0}, @{id=$iAyran;qty=12;price=55;disc=0}, @{id=$iWater;qty=5;price=55;disc=0})
$po9 = CreatePO $pre3H "WF-PRE3-003" $custSam "Cash" @(@{id=$iCheese;qty=5;price=200;disc=10})
log "Pre3: $po7, $po8, $po9"

$allPoIds = @($po1,$po2,$po3,$po4,$po5,$po6,$po7,$po8,$po9)
log "Total pre-orders: $($allPoIds.Count)"

# ========== STEP 4: Manager approves + converts to SO ==========
log "=== STEP 4: Manager Approves + Converts ==="
$allSoIds = @()
foreach ($pid in $allPoIds) {
    Invoke-RestMethod "http://localhost:5134/api/v1/pre-orders/$pid/submit" -Method POST -Headers $adminH | Out-Null
    Invoke-RestMethod "http://localhost:5134/api/v1/pre-orders/$pid/approve" -Method POST -Headers $adminH | Out-Null
    $sid = (Invoke-RestMethod "http://localhost:5134/api/v1/pre-orders/$pid/convert" -Method POST -Headers $adminH).Trim('"')
    $allSoIds += $sid
    log "  PO $pid -> SO $sid"
}

# ========== STEP 5: Confirm all Sales Orders ==========
log "=== STEP 5: Confirming Sales Orders ==="
foreach ($sid in $allSoIds) {
    Invoke-RestMethod "http://localhost:5134/api/v1/sales-orders/$sid/confirm" -Method POST -Headers $adminH | Out-Null
}
log "All 9 SOs confirmed"

# ========== STEP 6: Get Driver Employee IDs ==========
$emps = Invoke-RestMethod "http://localhost:5134/api/v1/employees" -Headers $adminH
$drvAId = ($emps | Where-Object { $_.code -eq "DRV-001" }).id
$drvBId = ($emps | Where-Object { $_.code -eq "DRV-002" }).id
$drvCId = ($emps | Where-Object { $_.code -eq "DRV-003" }).id
log "Driver Emp IDs: $drvAId, $drvBId, $drvCId"

# ========== STEP 7: Create Routes with Stops + Shipments ==========
log "=== STEP 7: Creating Routes ==="

function CreateRouteWithStops($number, $date, $driverEmpId, $soIds) {
    $rBody = "{""code"":""$number"",""date"":""$date"",""driverEmployeeId"":""$driverEmpId"",""notes"":""E2E workflow test""}"
    $routeId = (Invoke-RestMethod "http://localhost:5134/api/v1/routes" -Method POST -Body $rBody -ContentType "application/json" -Headers $adminH).Trim('"')
    
    $stopNum = 1
    foreach ($sid in $soIds) {
        $soData = Invoke-RestMethod "http://localhost:5134/api/v1/sales-orders/$sid" -Headers $adminH
        $custId = $soData.customerId
        $custName = $soData.customerName
        
        $sBody = "{""customerId"":""$custId"",""sequenceNumber"":$stopNum,""notes"":""Stop for $custName""}"
        $stopId = (Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeId/stops" -Method POST -Body $sBody -ContentType "application/json" -Headers $adminH).Trim('"')
        
        # Assign sales order as shipment
        $shBody = "{""salesOrderId"":""$sid""}"
        Invoke-RestMethod "http://localhost:5134/api/v1/routes/_/stops/$stopId/shipments" -Method POST -Body $shBody -ContentType "application/json" -Headers $adminH | Out-Null
        
        $stopNum++
    }
    return $routeId
}

# Route A (Ivan): 3 orders - 2 from Pre1 + 1 from Pre2
$routeA = CreateRouteWithStops "R-WF-A" "2026-06-21" $drvAId @($allSoIds[0],$allSoIds[1],$allSoIds[3])
log "Route A: $routeA"

# Route B (Petr): 4 orders - 1 from Pre1 + 2 from Pre2 + 1 from Pre3
$routeB = CreateRouteWithStops "R-WF-B" "2026-06-21" $drvBId @($allSoIds[2],$allSoIds[4],$allSoIds[5],$allSoIds[6])
log "Route B: $routeB"

# Route C (Sergey): 2 orders - 2 from Pre3
$routeC = CreateRouteWithStops "R-WF-C" "2026-06-21" $drvCId @($allSoIds[7],$allSoIds[8])
log "Route C: $routeC"

# ========== STEP 8: Drivers deliver ==========
log "=== STEP 8: Drivers Deliver ==="
function DriverDeliver($routeId, $driverUser, $driverPass) {
    $drvH = LoginAs $driverUser $driverPass
    
    # Start route
    Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeId/start" -Method POST -Headers $drvH | Out-Null
    $routeDetail = Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeId/detail" -Headers $adminH
    log "  Route $routeId started, stops: $($routeDetail.stops.Count)"
    
    # Complete all stops
    foreach ($stop in $routeDetail.stops) {
        $stopId = $stop.id
        # Arrive
        Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeId/stops/$stopId/arrive" -Method POST -Headers $drvH | Out-Null
        # Complete (auto-ships shipments)
        Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeId/stops/$stopId/complete" -Method POST -Headers $drvH | Out-Null
        log "    Stop $($stop.sequenceNumber) completed ($($stop.customerName))"
    }
    
    # Complete route
    Invoke-RestMethod "http://localhost:5134/api/v1/routes/$routeId/complete" -Method POST -Headers $drvH | Out-Null
    log "  Route $routeId completed"
}

# Driver A delivers Route A
DriverDeliver $routeA "driverA" "Drv1234!"

# Driver B delivers Route B
DriverDeliver $routeB "driverB" "Drv1234!"

# Driver C delivers Route C
DriverDeliver $routeC "driverC" "Drv1234!"

# ========== STEP 9: Verify ==========
log "=== STEP 9: Verification ==="

# Check all SOs are Shipped
$allSo = Invoke-RestMethod "http://localhost:5134/api/v1/sales-orders" -Headers $adminH
$wfSos = $allSo | Where-Object { $_.number -like "SO-WF-*" }
log "Workflow SOs: $($wfSos.Count)"
foreach ($so in $wfSos) { log "  $($so.number): $($so.status)" }
$shippedCount = ($wfSos | Where-Object { $_.status -eq "Shipped" }).Count
log "Shipped: $shippedCount / $($wfSos.Count)"

# Check inventory
$items = Invoke-RestMethod "http://localhost:5134/api/v1/inventory-items" -Headers $adminH
$bread = ($items | Where-Object { $_.id -eq $iBread }).quantityOnHand
$milk = ($items | Where-Object { $_.id -eq $iMilk }).quantityOnHand
$cheese = ($items | Where-Object { $_.id -eq $iCheese }).quantityOnHand
$ayran = ($items | Where-Object { $_.id -eq $iAyran }).quantityOnHand
$water = ($items | Where-Object { $_.id -eq $iWater }).quantityOnHand
log "Final stock: Bread=$bread Milk=$milk Cheese=$cheese Ayran=$ayran Water=$water"

# Check movement history
log "Movement history for Bread Wheat:"
$mov = Invoke-RestMethod "http://localhost:5134/api/v1/reports/item-movements?itemId=$iBread&warehouseId=$whDepo1" -Headers $adminH
$mov | ForEach-Object { log "  $($_.operationType) qty=$($_.quantityChange) balance=$($_.runningBalance) doc=$($_.documentNumber)" }

log "=== E2E WORKFLOW COMPLETE ==="
