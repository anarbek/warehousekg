# Mobile App

Flutter mobile client (`warehousekg_mobile`) — offline-first inventory auditing for warehouse workers.

**Status**: v1 implemented ✅ (2026-06-19)

---

## Architecture

```
warehousekg_mobile/
├── lib/
│   ├── main.dart                          # ProviderScope + WarehouseKgApp
│   ├── app/
│   │   ├── app.dart                       # GoRouter with auth redirect & sync service init
│   │   └── routes.dart                    # Route definitions
│   ├── core/
│   │   ├── api/
│   │   │   ├── api_client.dart            # Dio with JWT interceptor + secure storage
│   │   │   └── api_constants.dart         # Backend URL & endpoint constants
│   │   └── network/
│   │       ├── connectivity.dart          # isOnlineProvider (connectivity_plus)
│   │       └── sync_service.dart          # Auto-sync PendingSync audits via timer
│   └── features/
│       ├── auth/
│       │   ├── login_screen.dart          # Username/password login
│       │   └── auth_repository.dart       # AuthNotifier: login/logout/checkAuth
│       ├── dashboard/
│       │   └── dashboard_screen.dart       # 6 menu cards (5 disabled), offline indicator
│       └── audit/
│           ├── audit_list_screen.dart      # Local + remote audit list, new audit dialog
│           ├── audit_detail_screen.dart    # Audit summary + line items + variance
│           ├── audit_count_screen.dart     # Item counting with search/filter/pause/resume
│           ├── audit_repository.dart       # Online/offline/sync methods
│           └── models/
│               └── audit_model.dart        # AuditModel, AuditLine, Warehouse, etc.
│       └── dispatching/
│           ├── dispatching_repository.dart  # Route/stop/shipment API methods + models
│           ├── screens/
│           │   ├── route_list_screen.dart   # Driver's route list
│           │   ├── route_detail_screen.dart # Route detail + stops
│           │   └── stop_detail_screen.dart  # Stop detail + shipments + workflow
│           └── models/
│               └── dispatching_models.dart  # RouteModel, StopModel, ShipmentModel
```

---

## Dependencies

| Package | Purpose |
|---|---|
| `flutter_riverpod` 2.x | State management |
| `go_router` | Declarative routing with auth guards |
| `dio` | HTTP client with JWT interceptor |
| `connectivity_plus` | Online/offline detection |
| `flutter_secure_storage` | Secure JWT & credentials storage |
| `drift` / `sqlite3` | Local SQLite database (listed, in-memory for v1) |

---

## Routes

| Path | Screen | Description |
|---|---|---|
| `/login` | LoginScreen | Username/password auth |
| `/dashboard` | DashboardScreen | 6 module cards, offline badge |
| `/audits` | AuditListScreen | Merged local + remote audit list |
| `/audits/:id` | AuditDetailScreen | Summary, progress bar, line items with variance |
| `/audits/:id/count` | AuditCountScreen | Item counting with search, filter, pause |
| `/dispatching` | RouteListScreen | Driver's route list (my routes only) |
| `/dispatching/routes/:id` | RouteDetailScreen | Route detail with stop list + status badges |
| `/dispatching/routes/:routeId/stops/:stopId` | StopDetailScreen | Stop detail with shipment list + workflow buttons |

**Auth guard**: GoRouter redirects unauthenticated users to `/login`.

---

## Features Implemented

### Auth Flow
- Login with username/password against `POST /api/v1/auth/login`
- JWT stored in `flutter_secure_storage`
- `AuthNotifier` manages auth state via Riverpod
- Auto-redirect after login/expiry

### Dashboard
- 6 module cards: Склад, Закупки, Продажи, Персонал, Отчёты, Аудиты, Доставка
- Active cards: **Аудиты**, **Доставка** (green badge); others disabled (grey)
- Offline indicator (cloud-off icon) when `isOnlineProvider` is false

### Audit CRUD
- **Create**: Dialog with warehouse dropdown (fetched from `GET /api/v1/warehouses`), notes field, shows item count before creating
- **Read**: Merged list of local (Draft/PendingSync/Synced) + remote (backend) audits
  - Remote audits fetched from `GET /api/v1/stock-audits`
  - Deduplication by audit `number`
  - Shows: status badge, warehouse, line count, counted items, total variance, employee name
- **Detail**: Tapping any audit shows summary + per-line breakdown:
  - Item name, SKU, system quantity, counted quantity (blue), variance (red/green)
  - Progress bar (counted / total)
  - Buttons: Начать подсчёт / Продолжить, Завершить аудит (local drafts only)
- **Delete**: Swipe-left to delete local audits with confirmation dialog
- **Refresh**: 🔄 button in app bar to reload remote audit list

### Offline Counting
- Per-warehouse inventory loaded from `GET /api/v1/reports/warehouse-stock?warehouseId=...`
- Items presented as cards with: name, SKU, system quantity
- Text input for counted quantity (numeric keypad)
- **Search**: filter items by name or SKU
- **Filter tabs**: Все, Не посчитано, Посчитано, Расхождения
- **Color coding**: green background if counted with no variance, orange if variance ≠ 0
- **Pause**: Back arrow returns to detail screen (preserves counted values)
- **Resume**: Continue button reopens count screen with previous values intact

### Driver Dispatching (v1, 2026-06-20)
- **Route list**: `GET /api/v1/routes/my` — shows routes assigned to the current driver (Planned, InProgress, Completed)
- **Route detail**: `GET /api/v1/routes/my/{id}/detail` — full route with stops, shipments, status badges
- **Stop detail**: Tapping a stop shows shipments, quantities, customer info
- **Workflow buttons**: Start route, complete route, arrive at stop, complete stop (hidden for Completed routes)
- **Offline indicator**: connectivity bar at top
- **JWT claim**: Uses `employee_id` claim to filter routes; set at login for users linked to Employee record
- **Models**: `RouteModel`, `RouteDetailModel`, `StopModel`, `ShipmentModel` in `dispatching_models.dart`

### Sync
1. User taps "Завершить" → status changes to `PendingSync`
2. `syncAudit()`: creates audit on backend via `POST /api/v1/stock-audits` with `systemQuantity` included, saves as Draft
3. Status updated to `Synced` locally — audit appears in web app as Draft, ready for authorized user to complete
4. Backend prevents duplicate audits for same warehouse+day (guards in `CreateStockAudit` handler and `audit_repository.createLocalAudit`)
5. Audit counter auto-initializes from existing backend numbers to avoid collisions

### Connectivity
- `isOnlineProvider` watches network state via `connectivity_plus`
- Offline audits created locally, synced when online
- `SyncService` timer polls for `PendingSync` audits (every 30s)

---

## Backend API Endpoints Used

| Endpoint | Method | Purpose |
|---|---|---|
| `/api/v1/auth/login` | POST | Authentication |
| `/api/v1/warehouses` | GET | List warehouses (cached) |
| `/api/v1/reports/warehouse-stock` | GET | Per-warehouse stock with quantities |
| `/api/v1/stock-audits` | GET | List all audits (remote list) |
| `/api/v1/stock-audits` | POST | Create audit (sync) |
| `/api/v1/stock-audits/{id}` | GET | Fetch audit detail |
| `/api/v1/stock-audits/{id}/complete` | POST | Complete audit (web-only, requires stock-audits-complete:write) |
| `/api/v1/routes/my` | GET | Driver's assigned routes |
| `/api/v1/routes/my/{id}/detail` | GET | Full route detail with stops + shipments |
| `/api/v1/routes/{id}/start` | POST | Start route (Planned → InProgress) |
| `/api/v1/routes/{id}/complete` | POST | Complete route (InProgress → Completed) |
| `/api/v1/routes/{routeId}/stops/{stopId}/arrive` | POST | Arrive at stop (Pending → InProgress) |
| `/api/v1/routes/{routeId}/stops/{stopId}/complete` | POST | Complete stop (InProgress → Completed, auto-ships) |
| `/api/v1/routes/{routeId}/stops/{stopId}/skip` | POST | Skip stop |

---

## Known Limitations (v1)

1. **In-memory storage**: Audit data stored in Dart lists; lost on app restart or hot restart. Drift SQLite listed as dependency but not yet wired.
2. **Hardcoded backend URL**: `http://10.0.2.2:5134` for Android emulator. Physical device requires actual server IP.
3. **Offline login**: Requires at least one online auth per session.
4. **No photo attachments**: Planned for v2.
5. **No barcode scanning**: Planned for v2.
6. **No push notifications**: Planned for v2.
7. **Audit list auto-load**: Remote audits fetched on screen load only; manual refresh needed for updates.

---

## Development

```bash
# Navigate to mobile project
cd warehousekg_mobile

# Run on Android emulator
flutter run -d emulator-5554

# Hot reload (preserves state)
r

# Hot restart (clears state)
Shift+R  or  R  (uppercase)
```

**Backend connection**: Android emulator accesses host machine at `10.0.2.2`. Backend must be running on `http://localhost:5134`.
