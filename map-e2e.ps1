$t=(Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method POST -Body '{"userName":"admin","password":"Admin1234!"}' -ContentType "application/json").accessToken
$h=@{Authorization="Bearer $t";"X-Tenant-Id"="ffffffff-ffff-ffff-ffff-ffffffffffff"}

# Update customers with Bishkek coordinates
$custData = @(
    @{id="5ed63d5e-6cc9-4611-85ed-c0c885320429"; lat=42.8746; lng=74.5698; addr="Bishkek, Chuy 123"},
    @{id="c6fb2277-eafa-497d-a168-75e90a13121b"; lat=42.8629; lng=74.5392; addr="Bishkek, Manas 456"},
    @{id="51c8406c-8c6c-46c5-a87e-8c18c20d7885"; lat=42.8820; lng=74.5826; addr="Bishkek, Sovietskaya 789"}
)

foreach ($cd in $custData) {
    $c = Invoke-RestMethod "http://localhost:5134/api/v1/customers/$($cd.id)" -Headers $h
    $body = '{"code":"'+$c.code+'","name":"'+$c.name+'","contactName":null,"email":null,"phone":null,"address":"'+$cd.addr+'","taxId":null,"latitude":'+$cd.lat+',"longitude":'+$cd.lng+',"isActive":true}'
    Invoke-RestMethod "http://localhost:5134/api/v1/customers/$($cd.id)" -Method PUT -Body $body -ContentType "application/json" -Headers $h | Out-Null
    Write-Host "Updated: $($c.name) -> ($($cd.lat), $($cd.lng))"
}

# Create fresh route with coords
$drvB = "af98e028-c467-4efd-8c1d-3065b9e6157e"
$ts = Get-Date -Format "HHmmss"
$code = "R-MAP-$ts"
$rBody = '{"code":"'+$code+'","date":"2026-06-21T00:00:00Z","driverEmployeeId":"'+$drvB+'","notes":"Map test route with coordinates"}'
$route = (Invoke-RestMethod "http://localhost:5134/api/v1/routes" -Method POST -Body $rBody -ContentType "application/json" -Headers $h).Trim('"')
Write-Host "Route: $route ($code)"

# Create stops with lat/lng
$sn=1
foreach ($cd in $custData) {
    $sBody = '{"routeId":"'+$route+'","sequenceNumber":'+$sn+',"customerId":"'+$cd.id+'","address":"'+$cd.addr+' ('+$cd.lat+', '+$cd.lng+')","latitude":'+$cd.lat+',"longitude":'+$cd.lng+',"notes":"Stop '+$sn+'"}'
    $stopId = (Invoke-RestMethod "http://localhost:5134/api/v1/routes/$route/stops" -Method POST -Body $sBody -ContentType "application/json" -Headers $h).Trim('"')
    Write-Host "  Stop $sn : $stopId"
    $sn++
}

# Verify
$rd = Invoke-RestMethod "http://localhost:5134/api/v1/routes/$route/detail" -Headers $h
Write-Host "Stops: $($rd.stops.Count)"
Write-Host "URL: http://localhost:4200/dispatching/routes/$route"
