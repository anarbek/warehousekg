# Roadmap

Planned features, milestones, and the prioritized backlog over time.

## Completed

- **Superadmin tenant management** ✅ — Full tenant CRUD with demo data seeding, suspend/activate workflow,
  `RequireSuperadmin` auth policy, Angular module at `/superadmin/tenants`.
  Superadmin users have `TenantId = Guid.Empty` (no tenant scope). See [[2026-06-21]].
- **Authorization rollout** ✅ — All controllers use resource-based policies with hybrid RBAC + PBAC.
  Tenant permission matrix with `CanRead`/`CanWrite`/`CanDelete` per role per resource. Admin UI at
  `/admin/permissions` and `/admin/users`. See [[04-Auth-Flow]].
- **Delete policy** ✅ — DELETE endpoints use dedicated `{resource}:delete` policies (separate from write).
  Frontend conditionally hides delete buttons via `PermissionsService`.
- **Scenario permissions** ✅ — `add-items-back-in-time` (with configurable `MaxBackdateDays`) and
  `stock-receipts-delete-completed` for business-logic gating beyond CRUD.
- **Transaction date** ✅ — Stock receipts have a `transactionDate` picker. Backdating is gated by
  `add-items-back-in-time` permission.
- **Error toast** ✅ — Centralized `ErrorToastService` shows actual backend error messages in popup
  notifications instead of generic inline text.

## Open items
- Main site where user can register and create a Tenant himself, there should be basic questionnaire where he should be able to select which features he wants
and app should create a basic setup with admin user, some categories and other basic data so the app is not empty when he logs in, content should be in 4 languages, CEO optimized
- Full live testing
- CI/CD to contabo's server
- Jenkins install on contabo
- GPS tracking of driver
- Report designing using Devexpress
- XML import capability to accept different formats of XML files(CCI, EFS, Pepsi, Philip Morris etc)
- Turkish and english languages in admin panel
- currently money type is hardcoded(KGS) it should be selecteable from dropdown, and there should be a menu to manage exchanges. so when user creates a receipt he should be able to select exchange and money type, exchange management screen should contain exchange rates management too
- **Preseller module** ✅ — Pre-order workflow: Draft → Submitted → Approved/Rejected → Converted to SalesOrder.
  Warehouse stock visibility with live difference calculation. PaymentType lookup table with 4 seeded types.
  Full Angular web module (list/form/detail) at `/preseller/pre-orders`. Full Flutter mobile module with
  my-orders list, create form with stock visibility, and detail screen. `Preseller` role with dedicated
  permissions. 13 integration tests. See [[2026-06-21]].
- **Mobile preseller module** ✅ — Flutter screens: PreOrderList, PreOrderForm (customer/warehouse/payment-type
  dropdowns, item picker with stock diff, date picker, save), PreOrderDetail (status chip, submit button).
  Dashboard tile "Предзаказы" enabled. See [[2026-06-21]].
- Dispatchers should be able creating invoices from web app, it should create a printeable invoice which can be printed and signed by customer, printers are old dot based printers, used by warehouse workers, invoices should be designeable in separate module using devexpress report designer

### Flutter Mobile App — `warehousekg_mobile`

> See [[2026-06-19]] for session log.

**v1 — Audit Module** ✅ (implemented 2026-06-19)
- Offline-first inventory auditing for warehouse workers
- Login → Dashboard (all modules disabled except Audit) → Start/Pause/Resume → Count → Complete → Sync
- Works without internet; syncs to backend on completion
- In-memory storage with Riverpod + Dio + JWT auth
- Per-warehouse stock using `GET /api/v1/reports/warehouse-stock`
- System quantity preserved during sync (no backend recalculation mismatch)
- Remote audit list with detail view (lines, variance, employee name)

**v2+ — Excluded from v1**
- Barcode scanning (camera integration) ✅ (implemented 2026-06-20)
- Photo attachments for audit items
- Push notifications for pending sync / audit assignments
- Offline login (currently requires online auth at least once per session)
- Full warehouse management modules (receipts, transfers, shipments)
- Reports and analytics in-app

**Driver Dispatching** ✅ (implemented 2026-06-20)
- Driver role with `employee_id` JWT claim
- `GET /api/v1/routes/my` — driver's own route list (Planned, InProgress, Completed)
- `GET /api/v1/routes/my/{id}/detail` — full route with stops + shipments
- Route workflow: start → arrive at stops → complete stops (auto-ship sales orders) → complete route
- Inventory auto-deduction on stop completion
- Flutter screens: RouteList, RouteDetail, StopDetail
- Dashboard "Доставка" tile enabled for all authenticated users
