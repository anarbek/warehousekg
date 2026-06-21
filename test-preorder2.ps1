$ErrorActionPreference = "Stop"
$body = @{ username="pre1"; password="Pre11234!" } | ConvertTo-Json
try {
  $login = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method Post -Body $body -ContentType "application/json" -UseBasicParsing
  $token = $login.accessToken
  Write-Host "Login OK. Token: $($token.Substring(0,20))..."
  
  $lines = @()
  $lines += @{ inventoryItemId = "e2e00000-0000-0000-0000-000000000001"; quantity = 5; unitPrice = 3.0; discountPercent = 0 }
  
  $preorder = @{
    number = "TEST-DT-FIX"
    customerId = "5ed63d5e-6cc9-4611-85ed-c0c885320429"
    warehouseId = "9e7938a6-2e68-4e03-b3b3-2aa6ca5b3fb8"
    paymentType = "CASH"
    currency = "KGS"
    expectedDateUtc = "2026-06-25T00:00:00.000"
    notes = "test"
    lines = $lines
  } | ConvertTo-Json -Depth 5
  
  Write-Host "Sending..."
  $result = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders" -Method Post -Body $preorder -ContentType "application/json" -Headers @{ Authorization="Bearer $token" } -UseBasicParsing
  Write-Host "Created: $result"
} catch {
  Write-Host "ERROR: $($_.Exception.Response.StatusCode.value__)"
  try {
    $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
    $resp = $reader.ReadToEnd()
    $reader.Close()
    Write-Host "Response: $resp"
  } catch {
    Write-Host $_.Exception.Message
  }
}
