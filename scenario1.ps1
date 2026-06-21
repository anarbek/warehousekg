$r = Get-Content d:\Projects\warehousekg\seed-result.txt
$final = ($r | Select-String "FINAL:").Line
$custId = $final.Split('|')[0]
$whId = $final.Split('|')[1]
$i1 = $final.Split('|')[2]
$i2 = $final.Split('|')[3]
$i3 = $final.Split('|')[4]

$log = "d:\Projects\warehousekg\scenario1-result.txt"
"" > $log

$token = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method POST -Body '{"userName":"admin","password":"Admin1234!"}' -ContentType "application/json").accessToken
$h = @{Authorization = "Bearer $token"; "X-Tenant-Id" = "ffffffff-ffff-ffff-ffff-ffffffffffff"}

"IDs: cust=$custId wh=$whId i1=$i1 i2=$i2 i3=$i3" >> $log

# Create pre-order
$body = "{""number"":""PO-E2E-001"",""customerId"":""$custId"",""warehouseId"":""$whId"",""paymentType"":""Cash"",""currency"":""KGS"",""lines"":[{""inventoryItemId"":""$i1"",""quantity"":10,""unitPrice"":100,""discountPercent"":5},{""inventoryItemId"":""$i2"",""quantity"":20,""unitPrice"":80,""discountPercent"":0},{""inventoryItemId"":""$i3"",""quantity"":5,""unitPrice"":200,""discountPercent"":10}]}"
$poId = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders" -Method POST -Body $body -ContentType "application/json" -Headers $h).Trim('"')
"PO ID: $poId" >> $log

# Verify draft
$po = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$poId" -Headers $h
"STATUS: $($po.status)" >> $log
"LINES: $($po.lines.Count)" >> $log
"TOTAL: $($po.totalAmount)" >> $log
foreach ($l in $po.lines) { "  LINE: $($l.inventoryItemName) qty=$($l.quantity) stock=$($l.warehouseStockSnapshot) diff=$($l.stockDifference) total=$($l.lineTotal)" >> $log }

# Submit
Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$poId/submit" -Method POST -Headers $h
$po = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$poId" -Headers $h
"SUBMIT -> $($po.status)" >> $log

# Approve
Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$poId/approve" -Method POST -Headers $h
$po = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$poId" -Headers $h
"APPROVE -> $($po.status)" >> $log

# Convert
$soId = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$poId/convert" -Method POST -Headers $h).Trim('"')
$po = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$poId" -Headers $h
"CONVERT -> $($po.status) -> SO: $soId" >> $log

# Ship
Invoke-RestMethod -Uri "http://localhost:5134/api/v1/sales-orders/$soId/confirm" -Method POST -Headers $h
Invoke-RestMethod -Uri "http://localhost:5134/api/v1/sales-orders/$soId/ship" -Method POST -Headers $h
$so = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/sales-orders/$soId" -Headers $h
"SHIP -> SO: $($so.status)" >> $log

# Final stock
$items = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/inventory-items" -Headers $h
$s1 = ($items | Where-Object { $_.id -eq $i1 }).quantityOnHand
$s2 = ($items | Where-Object { $_.id -eq $i2 }).quantityOnHand
$s3 = ($items | Where-Object { $_.id -eq $i3 }).quantityOnHand
"FINAL STOCK: Bread=$s1 Milk=$s2 Cheese=$s3" >> $log
"SCENARIO 1 DONE" >> $log

Get-Content $log
