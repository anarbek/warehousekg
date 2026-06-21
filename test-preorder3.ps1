param()

$ErrorActionPreference = "Continue"

# Login as admin
$loginBody = @{ username = "admin"; password = "Admin1234!" } | ConvertTo-Json -Compress
Write-Host "Logging in as admin..."
$login = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $login.accessToken
Write-Host "Login OK. Token length: $($token.Length)"

# Test 1: Without expectedDateUtc
Write-Host "`nTest 1: Without expectedDateUtc..."
$lines1 = @(
    @{ inventoryItemId = "e2e00000-0000-0000-0000-000000000001"; quantity = 5; unitPrice = 3.0; discountPercent = 0 }
)
$body1 = @{
    number = "TEST-NODATE"
    customerId = "5ed63d5e-6cc9-4611-85ed-c0c885320429"
    warehouseId = "9e7938a6-2e68-4e03-b3b3-2aa6ca5b3fb8"
    paymentType = "CASH"
    currency = "KGS"
    notes = "test no date"
    lines = $lines1
} | ConvertTo-Json -Depth 3 -Compress

try {
    $r = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders" -Method Post -Body $body1 -ContentType "application/json" -Headers @{ Authorization = "Bearer $token" }
    Write-Host "SUCCESS: $r"
} catch {
    $code = $_.Exception.Response.StatusCode.value__
    Write-Host "FAILED: $code"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $resp = $reader.ReadToEnd()
        $reader.Close()
        Write-Host "Body: $resp"
    }
}

# Test 2: With expectedDateUtc
Write-Host "`nTest 2: With expectedDateUtc..."
$lines2 = @(
    @{ inventoryItemId = "e2e00000-0000-0000-0000-000000000001"; quantity = 3; unitPrice = 2.0; discountPercent = 0 }
)
$body2 = @{
    number = "TEST-WITHDATE"
    customerId = "5ed63d5e-6cc9-4611-85ed-c0c885320429"
    warehouseId = "9e7938a6-2e68-4e03-b3b3-2aa6ca5b3fb8"
    paymentType = "CASH"
    currency = "KGS"
    expectedDateUtc = "2026-06-25T00:00:00.000"
    notes = "test with date"
    lines = $lines2
} | ConvertTo-Json -Depth 3 -Compress

try {
    $r = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders" -Method Post -Body $body2 -ContentType "application/json" -Headers @{ Authorization = "Bearer $token" }
    Write-Host "SUCCESS: $r"
} catch {
    $code = $_.Exception.Response.StatusCode.value__
    Write-Host "FAILED: $code"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $resp = $reader.ReadToEnd()
        $reader.Close()
        Write-Host "Body: $resp"
    }
}
