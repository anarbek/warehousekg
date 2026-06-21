param()
$ErrorActionPreference = "Continue"
$base = "http://localhost:5134/api/v1"

Write-Host "╔══════════════════════════════════════╗"
Write-Host "║   PRE-ORDER E2E TEST (as pre1)      ║"
Write-Host "╚══════════════════════════════════════╝"

# Step 1: Login
Write-Host "`n[1/6] Logging in as pre1..."
$loginBody = @{ username="pre1"; password="Preseller1234!" } | ConvertTo-Json -Compress
$login = Invoke-RestMethod -Uri "$base/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $login.accessToken
$headers = @{ Authorization="Bearer $token" }
Write-Host "  OK - token: $($token.Substring(0,20))..."

# Step 2: Load ref data
Write-Host "`n[2/6] Loading reference data..."
$warehouses = Invoke-RestMethod -Uri "$base/warehouses" -Headers $headers
$customers = Invoke-RestMethod -Uri "$base/customers" -Headers $headers
$items = Invoke-RestMethod -Uri "$base/inventory-items" -Headers $headers
$payTypes = Invoke-RestMethod -Uri "$base/payment-types" -Headers $headers
Write-Host "  Warehouses: $($warehouses.Count) | Customers: $($customers.Count) | Items: $($items.Count) | PaymentTypes: $($payTypes.Count)"

if ($warehouses.Count -eq 0 -or $customers.Count -eq 0 -or $items.Count -eq 0) {
    Write-Host "  ERROR: Missing reference data!"
    exit 1
}

# Pick the first available warehouse, customer, and items
$wh = $warehouses[0]
$cust = $customers[0]
$item1 = $items | Where-Object { $_.sku -eq "AYR" } | Select-Object -First 1
$item2 = $items | Where-Object { $_.sku -eq "E2E-002" } | Select-Object -First 1

if (-not $item1) { $item1 = $items[0] }
if (-not $item2) { $item2 = $items[1] }

Write-Host "  Selected warehouse: $($wh.name) ($($wh.id))"
Write-Host "  Selected customer: $($cust.name) ($($cust.id))"
Write-Host "  Selected items: $($item1.name) ($($item1.id)), $($item2.name) ($($item2.id))"

# Step 3: Create draft pre-order (Flutter-format: paymentType as name, date without Z)
Write-Host "`n[3/6] Creating draft pre-order..."
$lines = @(
    @{ inventoryItemId = $item1.id; quantity = 5; unitPrice = 2.0; discountPercent = 0 },
    @{ inventoryItemId = $item2.id; quantity = 5; unitPrice = 3.0; discountPercent = 0 }
)
$preBody = @{
    number = "E2E-FLUTTER-001"
    customerId = $cust.id
    warehouseId = $wh.id
    paymentType = $payTypes[0].name
    currency = "KGS"
    expectedDateUtc = "2026-06-22T00:00:00.000"
    notes = "E2E test from API"
    lines = $lines
} | ConvertTo-Json -Depth 3 -Compress

try {
    $preId = Invoke-RestMethod -Uri "$base/pre-orders" -Method Post -Body $preBody -ContentType "application/json" -Headers $headers
    Write-Host "  CREATED: $preId"
} catch {
    $code = $_.Exception.Response.StatusCode.value__
    Write-Host "  FAILED (Create): $code"
    $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
    $err = $reader.ReadToEnd(); $reader.Close()
    Write-Host "  $($err.Substring(0, [Math]::Min(500, $err.Length)))"
    exit 1
}

# Step 4: Submit
Write-Host "`n[4/6] Submitting pre-order..."
try {
    $r = Invoke-RestMethod -Uri "$base/pre-orders/$preId/submit" -Method Post -Headers $headers
    Write-Host "  SUBMITTED: $($r.StatusCode)"
} catch {
    $code = $_.Exception.Response.StatusCode.value__
    Write-Host "  FAILED (Submit): $code"
}

# Step 5: Get detail
Write-Host "`n[5/6] Getting pre-order detail..."
try {
    $detail = Invoke-RestMethod -Uri "$base/pre-orders/$preId" -Headers $headers
    Write-Host "  Number: $($detail.number)"
    Write-Host "  Status: $($detail.status)"
    Write-Host "  Customer: $($detail.customerName)"
    Write-Host "  Payment: $($detail.paymentType)"
    Write-Host "  Total: $($detail.totalAmount)"
    Write-Host "  Lines: $($detail.lines.Count)"
    foreach ($line in $detail.lines) {
        Write-Host "    - $($line.itemName): qty=$($line.quantity) price=$($line.unitPrice) total=$($line.lineTotal)"
    }
} catch {
    $code = $_.Exception.Response.StatusCode.value__
    Write-Host "  FAILED (GetById): $code"
}

# Step 6: Get my pre-orders list
Write-Host "`n[6/6] Getting my pre-orders..."
try {
    $myList = Invoke-RestMethod -Uri "$base/pre-orders/my" -Headers $headers
    Write-Host "  Found $($myList.Count) pre-orders"
    foreach ($po in $myList | Select-Object -First 5) {
        Write-Host "    $($po.number) - $($po.status) - $($po.totalAmount) KGS"
    }
} catch {
    $code = $_.Exception.Response.StatusCode.value__
    Write-Host "  FAILED (MyList): $code"
}

Write-Host "`n╔══════════════════════════════════════╗"
Write-Host "║   E2E TEST COMPLETE                  ║"
Write-Host "╚══════════════════════════════════════╝"
