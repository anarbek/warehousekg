$ErrorActionPreference = "Stop"

# Login
$body = @{userName="admin";password="Admin1234!"} | ConvertTo-Json
$resp = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method POST -Body $body -ContentType "application/json"
$token = $resp.accessToken
$headers = @{Authorization="Bearer $token"; "X-Tenant-Id"="ffffffff-ffff-ffff-ffff-ffffffffffff"}

Write-Host "=== Creating Customer ==="
try {
    $custBody = @{code="E2E-CUST";name="E2E Test Customer"} | ConvertTo-Json
    $custId = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/customers" -Method POST -Body $custBody -ContentType "application/json" -Headers $headers).Trim('"')
    Write-Host "Customer: $custId"
} catch {
    Write-Host "Customer may exist: $_"
}

Write-Host "=== Getting Warehouse ==="
$whs = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/warehouses" -Headers $headers
$whId = $whs[0].id
Write-Host "Warehouse: $whId"

Write-Host "=== Getting UOM ==="
$uoms = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/units-of-measure" -Headers $headers
$uomId = $uoms[0].id
Write-Host "UOM: $uomId"

Write-Host "=== Getting Category ==="
$cats = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/item-categories" -Headers $headers
$catId = $cats[0].id
Write-Host "Category: $catId"

Write-Host "=== Creating Inventory Items ==="
$skuList = @("E2E-BREAD","E2E-MILK","E2E-CHEESE","E2E-COLA","E2E-WATER")
$nameList = @("Хлеб пшеничный","Молоко 3.2%","Сыр Голландский","Кола 1л","Вода 5л")
$itemIds = @()
for ($i = 0; $i -lt 5; $i++) {
    try {
        $it = @{sku=$skuList[$i];name=$nameList[$i];categoryId=$catId;unitOfMeasureId=$uomId;quantityOnHand=0;isActive=$true} | ConvertTo-Json
        $id = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/inventory-items" -Method POST -Body $it -ContentType "application/json" -Headers $headers).Trim('"')
        $itemIds += $id
        Write-Host "  $($nameList[$i]): $id"
    } catch {
        Write-Host "  $($nameList[$i]): already exists"
        $existing = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/inventory-items" -Headers $headers | Where-Object { $_.sku -eq $skuList[$i] }
        $itemIds += $existing.id
    }
}

Write-Host "=== Adding Stock via Receipt ==="
$lines = @()
foreach ($iid in $itemIds) { $lines += @{inventoryItemId=$iid;quantity=100} }
$recBody = @{number="RCV-E2E-001";warehouseId=$whId;lines=$lines} | ConvertTo-Json -Depth 4
try {
    $recId = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/stock-receipts" -Method POST -Body $recBody -ContentType "application/json" -Headers $headers).Trim('"')
    Write-Host "Receipt: $recId"
    Invoke-RestMethod -Uri "http://localhost:5134/api/v1/stock-receipts/$recId/complete" -Method POST -Headers $headers
    Write-Host "Receipt completed!"
} catch { 
    Write-Host "Receipt may exist: $_" 
}

Write-Host "=== Stock Verification ==="
$stock = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/warehouse-stock?warehouseId=$whId" -Headers $headers
$stock | ForEach-Object { Write-Host "  $($_.name): $($_.quantityOnHand)" }

Write-Host "=== Payment Types ==="
$pts = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/payment-types" -Headers $headers
$pts | ForEach-Object { Write-Host "  $($_.name)" }

Write-Host "=== DONE: Test data seeded ==="
Write-Host "customerId=$($custId)"
Write-Host "warehouseId=$whId"
Write-Host "itemCount=$($itemIds.Count)"
