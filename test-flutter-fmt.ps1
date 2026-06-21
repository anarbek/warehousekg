param()
$ErrorActionPreference = "Continue"

$body = @{ username="pre1"; password="Preseller1234!" } | ConvertTo-Json -Compress
$login = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method Post -Body $body -ContentType "application/json"
$token = $login.accessToken
Write-Host "Login OK as pre1"

# Test with Flutter-like data: paymentType as display name, date without Z, real items
$lines = @(
    @{ inventoryItemId = "21ca97d7-53f7-4ccd-a98a-3af249e118c0"; quantity = 5.0; unitPrice = 2.0; discountPercent = 0 },
    @{ inventoryItemId = "b90df7e4-510c-4aa8-a1c1-8e42a448f7e3"; quantity = 5.0; unitPrice = 3.0; discountPercent = 0 }
)
$preBody = @{
    number = "TEST-FLUTTER-FMT"
    customerId = "5ed63d5e-6cc9-4611-85ed-c0c885320429"
    warehouseId = "6d835177-f5f1-4f01-a944-44fb2bafce43"
    paymentType = "Наличные"
    currency = "KGS"
    expectedDateUtc = "2026-06-22T00:00:00.000"
    notes = "test flutter format"
    lines = $lines
} | ConvertTo-Json -Depth 3 -Compress

Write-Host "Sending: $preBody"

try {
    $r = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders" -Method Post -Body $preBody -ContentType "application/json" -Headers @{ Authorization="Bearer $token" }
    Write-Host "SUCCESS: $r"
} catch {
    $code = $_.Exception.Response.StatusCode.value__
    Write-Host "FAILED: $code"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $resp = $reader.ReadToEnd()
        $reader.Close()
        if ($resp.Length -gt 800) { $resp = $resp.Substring(0, 800) + "..." }
        Write-Host "Body: $resp"
    }
}
