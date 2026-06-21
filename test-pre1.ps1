param()
$ErrorActionPreference = "Continue"

# Try passwords for pre1
$passwords = @("Pre11234!", "Preseller1234!", "pre1", "Pre1!")

foreach ($pw in $passwords) {
    $body = "{`"username`":`"pre1`",`"password`":`"$pw`"}"
    try {
        $login = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method Post -Body $body -ContentType "application/json"
        Write-Host "SUCCESS with password: $pw"
        $token = $login.accessToken
        
        # Now create a pre-order as pre1
        $preBody = '{"number":"TEST-PRE1-API","customerId":"5ed63d5e-6cc9-4611-85ed-c0c885320429","warehouseId":"6d835177-f5f1-4f01-a944-44fb2bafce43","paymentType":"CASH","currency":"KGS","notes":"test","lines":[{"inventoryItemId":"a4396dc7-ca44-4c7a-ac7f-ef67297fee9b","quantity":3,"unitPrice":1.0,"discountPercent":0}]}'
        
        try {
            $r = Invoke-RestMethod -Uri "http://localhost:5134/api/v1/pre-orders" -Method Post -Body $preBody -ContentType "application/json" -Headers @{ Authorization="Bearer $token" }
            Write-Host "Pre-order CREATED: $r"
        } catch {
            $code = $_.Exception.Response.StatusCode.value__
            Write-Host "Pre-order FAILED: $code"
            if ($_.Exception.Response) {
                $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                $resp = $reader.ReadToEnd()
                $reader.Close()
                if ($resp.Length -gt 500) { $resp = $resp.Substring(0, 500) + "..." }
                Write-Host "Body: $resp"
            }
        }
        break
    } catch {
        $code = $_.Exception.Response.StatusCode.value__
        Write-Host "Password '$pw' => $code"
    }
}
