param()
$ErrorActionPreference = "Continue"

# Login
$body = '{"username":"pre1","password":"Preseller1234!"}'
$login = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method Post -Body $body -ContentType "application/json"
$token = $login.accessToken
$h = @{ Authorization = "Bearer $token" }

Write-Host "=== E2E PRE-ORDER TEST (as pre1) ==="

# Load ref data
$wh = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/warehouses" -Headers $h)[0]
$cust = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/customers" -Headers $h)[0]
$items = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/inventory-items" -Headers $h
$ptypes = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/payment-types" -Headers $h
$item1 = $items | Where-Object { $_.sku -eq "AYR" } | Select-Object -First 1
$item2 = $items | Where-Object { $_.sku -eq "E2E-002" } | Select-Object -First 1

Write-Host "Warehouse: $($wh.name)"
Write-Host "Customer: $($cust.name)"
Write-Host "Items: $($item1.name), $($item2.name)"
Write-Host "PaymentType: $($ptypes[0].name)"

$num = "E2E-" + (Get-Date -Format "HHmmss")

# Create using a JSON file to avoid encoding issues
$json = @"
{
  "number": "$num",
  "customerId": "$($cust.id)",
  "warehouseId": "$($wh.id)",
  "paymentType": "$($ptypes[0].name)",
  "currency": "KGS",
  "expectedDateUtc": "2026-06-22T00:00:00.000",
  "notes": "E2E test",
  "lines": [
    {"inventoryItemId": "$($item1.id)", "quantity": 5, "unitPrice": 2.0, "discountPercent": 0},
    {"inventoryItemId": "$($item2.id)", "quantity": 5, "unitPrice": 3.0, "discountPercent": 0}
  ]
}
"@

Write-Host "`nSending: $json"

try {
    $preId = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders" -Method Post -Body $json -ContentType "application/json; charset=utf-8" -Headers $h
    Write-Host "`nCREATED: $preId"
    
    # Submit
    Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$preId/submit" -Method Post -Headers $h | Out-Null
    Write-Host "SUBMITTED"
    
    # Get detail
    $d = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/$preId" -Headers $h
    Write-Host "Status: $($d.status), Total: $($d.totalAmount), Lines: $($d.lines.Count)"
    
    # My list
    $my = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders/my" -Headers $h
    Write-Host "My pre-orders: $($my.Count)"
    
    Write-Host "`n*** E2E TEST PASSED ***"
} catch {
    Write-Host "FAILED: $($_.Exception.Response.StatusCode.value__)"
    $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
    $err = $reader.ReadToEnd(); $reader.Close()
    if ($err.Length -gt 500) { $err = $err.Substring(0, 500) + "..." }
    Write-Host $err
}
