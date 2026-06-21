param()
$ErrorActionPreference = "Continue"

$body = @{ username="pre1"; password="Preseller1234!" } | ConvertTo-Json -Compress
$login = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method Post -Body $body -ContentType "application/json"
$token = $login.accessToken
Write-Host "Login OK as pre1"

$jsonBody = Get-Content -Path "d:\Projects\warehousekg\test-body.json" -Raw -Encoding UTF8
Write-Host "Body bytes: $($jsonBody.Length)"
Write-Host "Contains Cyrillic: $($jsonBody -match 'Наличные')"

try {
    $r = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders" -Method Post -Body $jsonBody -ContentType "application/json; charset=utf-8" -Headers @{ Authorization="Bearer $token" }
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
