$resultFile = "d:\Projects\warehousekg\seed-result.txt"
"" > $resultFile

$token = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method POST -Body '{"userName":"admin","password":"Admin1234!"}' -ContentType "application/json").accessToken
"Token: $($token.Substring(0,20))..." >> $resultFile

$h = @{Authorization = "Bearer $token"; "X-Tenant-Id" = "ffffffff-ffff-ffff-ffff-ffffffffffff"}
$whId = "1ac52ff4-ac6c-486d-8666-f09308b3d0af"
$uomId = "72efb659-3fb7-4d7a-8e8e-4c4ce08a348d"
$catId = "03a88e32-7d04-4755-bdd7-bb34cd108844"

# Create customer
try { 
    $c = Invoke-RestMethod "http://localhost:5134/api/v1/customers" -Method POST -Body '{"code":"E2E-01","name":"E2E Customer"}' -ContentType "application/json" -Headers $h
    "CUSTOMER OK: $c" >> $resultFile
} catch { "CUSTOMER: $_" >> $resultFile }

# Get customer ID
$custs = Invoke-RestMethod "http://localhost:5134/api/v1/customers" -Headers $h
$custId = ($custs | Where-Object { $_.code -eq "E2E-01" }).id
"customerId=$custId" >> $resultFile

# Create items
$i1Json = "{`"sku`":`"E2E-001`",`"name`":`"Bread`",`"categoryId`":`"$catId`",`"unitOfMeasureId`":`"$uomId`",`"quantityOnHand`":0,`"isActive`":true}"
$i2Json = "{`"sku`":`"E2E-002`",`"name`":`"Milk`",`"categoryId`":`"$catId`",`"unitOfMeasureId`":`"$uomId`",`"quantityOnHand`":0,`"isActive`":true}"
$i3Json = "{`"sku`":`"E2E-003`",`"name`":`"Cheese`",`"categoryId`":`"$catId`",`"unitOfMeasureId`":`"$uomId`",`"quantityOnHand`":0,`"isActive`":true}"

try { $i1 = (Invoke-RestMethod "http://localhost:5134/api/v1/inventory-items" -Method POST -Body $i1Json -ContentType "application/json" -Headers $h).Trim('"'); "i1=$i1" >> $resultFile } catch { "i1: $_" >> $resultFile }
try { $i2 = (Invoke-RestMethod "http://localhost:5134/api/v1/inventory-items" -Method POST -Body $i2Json -ContentType "application/json" -Headers $h).Trim('"'); "i2=$i2" >> $resultFile } catch { "i2: $_" >> $resultFile }
try { $i3 = (Invoke-RestMethod "http://localhost:5134/api/v1/inventory-items" -Method POST -Body $i3Json -ContentType "application/json" -Headers $h).Trim('"'); "i3=$i3" >> $resultFile } catch { "i3: $_" >> $resultFile }

# Get actual IDs
$items = Invoke-RestMethod "http://localhost:5134/api/v1/inventory-items" -Headers $h
$i1 = ($items | Where-Object { $_.sku -eq "E2E-001" }).id
$i2 = ($items | Where-Object { $_.sku -eq "E2E-002" }).id
$i3 = ($items | Where-Object { $_.sku -eq "E2E-003" }).id
"itemIds=$i1,$i2,$i3" >> $resultFile

# Add stock receipt
$recBody = "{`"number`":`"RCV-E2E-01`",`"warehouseId`":`"$whId`",`"lines`":[{`"inventoryItemId`":`"$i1`",`"quantity`":50},{`"inventoryItemId`":`"$i2`",`"quantity`":80},{`"inventoryItemId`":`"$i3`",`"quantity`":60}]}"
try { 
    $recId = (Invoke-RestMethod "http://localhost:5134/api/v1/stock-receipts" -Method POST -Body $recBody -ContentType "application/json" -Headers $h).Trim('"')
    "recId=$recId" >> $resultFile
    Invoke-RestMethod "http://localhost:5134/api/v1/stock-receipts/$recId/complete" -Method POST -Headers $h
    "STOCK COMPLETED" >> $resultFile
} catch { "STOCK: $_" >> $resultFile }

# Verify
$stock = Invoke-RestMethod "http://localhost:5134/api/v1/pre-orders/warehouse-stock?warehouseId=$whId" -Headers $h
"stock_count=$($stock.Count)" >> $resultFile
"whId=$whId" >> $resultFile
foreach ($s in $stock) { "  $($s.name): $($s.quantityOnHand)" >> $resultFile }
"FINAL: $custId|$whId|$i1|$i2|$i3" >> $resultFile
"DONE" >> $resultFile
