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
and app should create a basic setup with admin user, some categories and other basic data so the app is not empty when he logs in
- Full live testing
- CI/CD to contabo's server
- Jenkins install on contabo
- GPS tracking of driver
- Presell module where preseller take preorders from customers and after approval those preorders become sales orders (with mobile module)
- Report designing using Devexpress
- XML import capability to accept different formats of XML files(CCI, EFS, Pepsi, Philip Morris etc)

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
