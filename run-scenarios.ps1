$ErrorActionPreference = "Continue"
$out = "d:\Projects\warehousekg\scenarios-result.txt"
"START" > $out

$token = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method POST -Body '{"userName":"admin","password":"Admin1234!"}' -ContentType "application/json").accessToken
$h = @{Authorization = "Bearer $token"; "X-Tenant-Id" = "ffffffff-ffff-ffff-ffff-ffffffffffff"}
$custId = "5ed63d5e-6cc9-4611-85ed-c0c885320429"
$whId = "1ac52ff4-ac6c-486d-8666-f09308b3d0af"
$i1 = "a4396dc7-ca44-4c7a-ac7f-ef67297fee9b"
$i2 = "b90df7e4-510c-4aa8-a1c1-8e42a448f7e3"
$i3 = "0f578667-e166-4fbc-b407-80f8ddd30e3f"

# === SCENARIO 1: Full lifecycle ===
"=== SCENARIO 1: Full Lifecycle ===" >> $out
$body = '{"number":"PO-E2E-001","customerId":"'+$custId+'","warehouseId":"'+$whId+'","paymentType":"Cash","currency":"KGS","lines":[{"inventoryItemId":"'+$i1+'","quantity":10,"unitPrice":100,"discountPercent":5},{"inventoryItemId":"'+$i2+'","quantity":20,"unitPrice":80,"discountPercent":0},{"inventoryItemId":"'+$i3+'","quantity":5,"unitPrice":200,"discountPercent":10}]}'
try {
    $poId = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders" -Method POST -Body $body -ContentType "application/json" -Headers $h).Trim('"')
    "S1-CREATE: $poId" >> $out

    # Verify draft
    $po = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$poId" -Headers $h
    "S1-DRAFT: status=$($po.status) lines=$($po.lines.Count) total=$($po.totalAmount)" >> $out

    # Submit
    Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$poId/submit" -Method POST -Headers $h | Out-Null
    $po = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$poId" -Headers $h
    "S1-SUBMIT: status=$($po.status)" >> $out

    # Approve
    Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$poId/approve" -Method POST -Headers $h | Out-Null
    $po = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$poId" -Headers $h
    "S1-APPROVE: status=$($po.status)" >> $out

    # Convert
    $soId = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$poId/convert" -Method POST -Headers $h).Trim('"')
    $po = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$poId" -Headers $h
    "S1-CONVERT: status=$($po.status) soId=$soId" >> $out

    # Confirm + Ship
    Invoke-RestMethod -Uri "http://localhost:5134/api/v1/sales-orders/$soId/confirm" -Method POST -Headers $h | Out-Null
    Invoke-RestMethod -Uri "http://localhost:5134/api/v1/sales-orders/$soId/ship" -Method POST -Headers $h | Out-Null
    $so = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/sales-orders/$soId" -Headers $h
    "S1-SHIP: soStatus=$($so.status)" >> $out
} catch { "S1-ERROR: $_" >> $out }

# === SCENARIO 2: Rejection ===
"=== SCENARIO 2: Rejection ===" >> $out
$body2 = '{"number":"PO-E2E-002","customerId":"'+$custId+'","warehouseId":"'+$whId+'","paymentType":"Card","currency":"USD","lines":[{"inventoryItemId":"'+$i1+'","quantity":5,"unitPrice":120,"discountPercent":0}]}'
try {
    $poId2 = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders" -Method POST -Body $body2 -ContentType "application/json" -Headers $h).Trim('"')
    "S2-CREATE: $poId2" >> $out
    Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$poId2/submit" -Method POST -Headers $h | Out-Null
    $po2 = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$poId2" -Headers $h
    "S2-SUBMIT: status=$($po2.status)" >> $out
    Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$poId2/reject" -Method POST -Body '{"reason":"Not enough stock at warehouse"}' -ContentType "application/json" -Headers $h | Out-Null
    $po2 = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$poId2" -Headers $h
    "S2-REJECT: status=$($po2.status) notes=$($po2.notes)" >> $out
} catch { "S2-ERROR: $_" >> $out }

# === SCENARIO 3: Create for mobile viewing ===
"=== SCENARIO 3: Mobile Pre-order ===" >> $out
$body3 = '{"number":"PO-E2E-003","customerId":"'+$custId+'","warehouseId":"'+$whId+'","paymentType":"Bank Transfer","currency":"KGS","lines":[{"inventoryItemId":"'+$i2+'","quantity":15,"unitPrice":85,"discountPercent":3},{"inventoryItemId":"'+$i3+'","quantity":8,"unitPrice":195,"discountPercent":0}]}'
try {
    $poId3 = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders" -Method POST -Body $body3 -ContentType "application/json" -Headers $h).Trim('"')
    "S3-CREATE: $poId3" >> $out
    $po3 = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$poId3" -Headers $h
    "S3-DRAFT: status=$($po3.status) lines=$($po3.lines.Count) total=$($po3.totalAmount)" >> $out
    foreach ($l in $po3.lines) { "  $($l.inventoryItemName) stock=$($l.warehouseStockSnapshot) qty=$($l.quantity) diff=$($l.stockDifference)" >> $out }
} catch { "S3-ERROR: $_" >> $out }

# Verify stock
$items = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/inventory-items" -Headers $h
$b1 = ($items | Where-Object { $_.id -eq $i1 }).quantityOnHand
$b2 = ($items | Where-Object { $_.id -eq $i2 }).quantityOnHand
$b3 = ($items | Where-Object { $_.id -eq $i3 }).quantityOnHand
"STOCK-FINAL: Bread=$b1 Milk=$b2 Cheese=$b3" >> $out
"DONE" >> $out
