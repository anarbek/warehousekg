$out = "d:\Projects\warehousekg\final-scenarios.txt"
"" > $out
$token = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method POST -Body '{"userName":"admin","password":"Admin1234!"}' -ContentType "application/json").accessToken
$h = @{Authorization = "Bearer $token"; "X-Tenant-Id" = "ffffffff-ffff-ffff-ffff-ffffffffffff"}
$c = "5ed63d5e-6cc9-4611-85ed-c0c885320429"
$w = "1ac52ff4-ac6c-486d-8666-f09308b3d0af"
$i1 = "a4396dc7-ca44-4c7a-ac7f-ef67297fee9b"
$i2 = "b90df7e4-510c-4aa8-a1c1-8e42a448f7e3"
$i3 = "0f578667-e166-4fbc-b407-80f8ddd30e3f"
$ts = Get-Date -Format "HHmmss"

# S1: Full lifecycle
$n1 = "PO-S1-$ts"
$body1 = "{""number"":""$n1"",""customerId"":""$c"",""warehouseId"":""$w"",""paymentType"":""Cash"",""currency"":""KGS"",""lines"":[{""inventoryItemId"":""$i1"",""quantity"":5,""unitPrice"":100,""discountPercent"":0},{""inventoryItemId"":""$i2"",""quantity"":10,""unitPrice"":80,""discountPercent"":5}]}"
try {
    $p1 = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders" -Method POST -Body $body1 -ContentType "application/json" -Headers $h).Trim('"')
    "S1: CREATED $p1 ($n1)" >> $out
    Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$p1/submit" -Method POST -Headers $h | Out-Null
    Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$p1/approve" -Method POST -Headers $h | Out-Null
    $so1 = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$p1/convert" -Method POST -Headers $h).Trim('"')
    "S1: CONVERTED to SO $so1" >> $out
    Invoke-RestMethod -Uri "http://localhost:5134/api/v1/sales-orders/$so1/confirm" -Method POST -Headers $h | Out-Null
    Invoke-RestMethod -Uri "http://localhost:5134/api/v1/sales-orders/$so1/ship" -Method POST -Headers $h | Out-Null
    "S1: SHIPPED" >> $out
} catch { "S1 ERROR: $_" >> $out }

# S2: Rejection
$n2 = "PO-S2-$ts"
$body2 = "{""number"":""$n2"",""customerId"":""$c"",""warehouseId"":""$w"",""paymentType"":""Card"",""currency"":""USD"",""lines"":[{""inventoryItemId"":""$i1"",""quantity"":3,""unitPrice"":120,""discountPercent"":0}]}"
try {
    $p2 = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders" -Method POST -Body $body2 -ContentType "application/json" -Headers $h).Trim('"')
    "S2: CREATED $p2 ($n2)" >> $out
    Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$p2/submit" -Method POST -Headers $h | Out-Null
    Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$p2/reject" -Method POST -Body '{"reason":"Stock insufficient at warehouse"}' -ContentType "application/json" -Headers $h | Out-Null
    $po2 = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$p2" -Headers $h
    "S2: REJECTED status=$($po2.status)" >> $out
} catch { "S2 ERROR: $_" >> $out }

# S3: Already created as PO-E2E-003 (f8f92232-c2f4-4ed6-aab2-943019aceac5)
"S3: ALREADY CREATED as PO-E2E-003 (draft with 2 lines)" >> $out

# List all
$all = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders" -Headers $h
"TOTAL: $($all.Count) pre-orders" >> $out
foreach ($p in $all) { "  $($p.number) status=$($p.status) customer=$($p.customerName) total=$($p.totalAmount)" >> $out }

# Final stock
$items = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/inventory-items" -Headers $h
"STOCK: Bread=$($items[0].quantityOnHand) Milk=$($items[1].quantityOnHand) Cheese=$($items[2].quantityOnHand)" >> $out
"DONE" >> $out
