param()
$ErrorActionPreference = "Continue"

$body = @{ username="pre1"; password="Preseller1234!" } | ConvertTo-Json -Compress
$login = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method Post -Body $body -ContentType "application/json"
$token = $login.accessToken

Write-Host "=== WAREHOUSES ==="
$r = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/warehouses" -Headers @{ Authorization="Bearer $token" }
$r | ForEach-Object { Write-Host "$($_.id) $($_.code) $($_.name)" }

Write-Host "`n=== CUSTOMERS ==="
$r2 = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/customers" -Headers @{ Authorization="Bearer $token" }
$r2 | ForEach-Object { Write-Host "$($_.id) $($_.code) $($_.name)" }

Write-Host "`n=== INVENTORY ITEMS ==="
$r3 = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/inventory-items" -Headers @{ Authorization="Bearer $token" }
$r3 | ForEach-Object { Write-Host "$($_.id) $($_.sku) $($_.name)" }
