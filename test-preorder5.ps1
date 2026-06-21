param()
$ErrorActionPreference = "Continue"

$token = (Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method Post -Body '{"username":"admin","password":"Admin1234!"}' -ContentType "application/json").accessToken
Write-Host "Login OK"

$body = '{"number":"TEST-REAL-ITEM","customerId":"5ed63d5e-6cc9-4611-85ed-c0c885320429","warehouseId":"6d835177-f5f1-4f01-a944-44fb2bafce43","paymentType":"CASH","currency":"KGS","expectedDateUtc":"2026-06-25T00:00:00.000Z","notes":"test","lines":[{"inventoryItemId":"a4396dc7-ca44-4c7a-ac7f-ef67297fee9b","quantity":5,"unitPrice":1.0,"discountPercent":0},{"inventoryItemId":"b90df7e4-510c-4aa8-a1c1-8e42a448f7e3","quantity":5,"unitPrice":3.0,"discountPercent":0}]}'

try {
    $r = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders" -Method Post -Body $body -ContentType "application/json" -Headers @{ Authorization="Bearer $token" }
    Write-Host "SUCCESS: $r"
} catch {
    Write-Host "FAIL: $($_.Exception.Response.StatusCode.value__)"
    $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
    $resp = $reader.ReadToEnd()
    $reader.Close()
    Write-Host "Body: $resp"
}
