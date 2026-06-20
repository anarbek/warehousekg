import urllib.request, json

# Login
data = json.dumps({'userName': 'driver', 'password': 'Driver1234!'}).encode()
req = urllib.request.Request('http://localhost:5134/api/v1/auth/login', data=data, headers={'Content-Type': 'application/json'}, method='POST')
resp = urllib.request.urlopen(req)
body = json.loads(resp.read())
token = body['accessToken']
auth = {'Authorization': 'Bearer ' + token}

# Get routes
req2 = urllib.request.Request('http://localhost:5134/api/v1/routes/my', headers=auth)
resp2 = urllib.request.urlopen(req2)
routes = json.loads(resp2.read())
route_id = routes[0]['id']
print('1. Start route...')
req3 = urllib.request.Request(f'http://localhost:5134/api/v1/routes/{route_id}/start', headers=auth, method='POST', data=b'')
resp3 = urllib.request.urlopen(req3)
print(f'   Start: {resp3.status}')

# Get detail with stops
req4 = urllib.request.Request(f'http://localhost:5134/api/v1/routes/my/{route_id}/detail', headers=auth)
resp4 = urllib.request.urlopen(req4)
detail = json.loads(resp4.read())
print(f'2. Route status: {detail["status"]}, stops: {len(detail["stops"])}')
for s in detail['stops']:
    print(f'   Stop {s["sequenceNumber"]}: {s["status"]} - {s.get("customerName","")}')

# Arrive and complete first stop
stop1 = detail['stops'][0]
print(f'3. Arrive at stop {stop1["sequenceNumber"]}...')
req5 = urllib.request.Request(f'http://localhost:5134/api/v1/routes/{route_id}/stops/{stop1["id"]}/arrive', headers=auth, method='POST', data=b'')
resp5 = urllib.request.urlopen(req5)
print(f'   Arrive: {resp5.status}')

print(f'4. Complete stop {stop1["sequenceNumber"]}...')
req6 = urllib.request.Request(f'http://localhost:5134/api/v1/routes/{route_id}/stops/{stop1["id"]}/complete', headers=auth, method='POST', data=b'')
resp6 = urllib.request.urlopen(req6)
print(f'   Complete: {resp6.status}')

# Arrive and complete second stop
stop2 = detail['stops'][1]
print(f'5. Arrive at stop {stop2["sequenceNumber"]}...')
req7 = urllib.request.Request(f'http://localhost:5134/api/v1/routes/{route_id}/stops/{stop2["id"]}/arrive', headers=auth, method='POST', data=b'')
resp7 = urllib.request.urlopen(req7)
print(f'   Arrive: {resp7.status}')

print(f'6. Complete stop {stop2["sequenceNumber"]}...')
req8 = urllib.request.Request(f'http://localhost:5134/api/v1/routes/{route_id}/stops/{stop2["id"]}/complete', headers=auth, method='POST', data=b'')
resp8 = urllib.request.urlopen(req8)
print(f'   Complete: {resp8.status}')

# Complete route
print('7. Complete route...')
req9 = urllib.request.Request(f'http://localhost:5134/api/v1/routes/{route_id}/complete', headers=auth, method='POST', data=b'')
resp9 = urllib.request.urlopen(req9)
print(f'   Complete: {resp9.status}')

# Verify final state
req10 = urllib.request.Request(f'http://localhost:5134/api/v1/routes/my/{route_id}/detail', headers=auth)
resp10 = urllib.request.urlopen(req10)
detail2 = json.loads(resp10.read())
print(f'\nFINAL: Route {detail2["code"]} = {detail2["status"]}')
for s in detail2['stops']:
    print(f'   Stop {s["sequenceNumber"]}: {s["status"]}')
