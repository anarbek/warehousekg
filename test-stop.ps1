$t=(Invoke-RestMethod -Uri "http://localhost:5134/api/v1/auth/login" -Method POST -Body '{"userName":"admin","password":"Admin1234!"}' -ContentType "application/json").accessToken
$h=@{Authorization="Bearer $t";"X-Tenant-Id"="ffffffff-ffff-ffff-ffff-ffffffffffff"}

# Create a test stop
$body='{"customerId":"5ed63d5e-6cc9-4611-85ed-c0c885320429","sequenceNumber":1,"notes":"test stop"}'
$stopId=(Invoke-RestMethod "http://localhost:5134/api/v1/routes/930e2bae-2ba6-4ed2-8fea-f9b0f1234a77/stops" -Method POST -Body $body -ContentType "application/json" -Headers $h).Trim('"')
Write-Host "STOP: $stopId"

# Check detail
$rd=Invoke-RestMethod "http://localhost:5134/api/v1/routes/930e2bae-2ba6-4ed2-8fea-f9b0f1234a77/detail" -Headers $h
Write-Host "STOPS: $($rd.stops.Count)"
