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
- **Preseller module** ✅ — Pre-order workflow: Draft → Submitted → Approved/Rejected → Converted to SalesOrder.
  Warehouse stock visibility with live difference calculation. PaymentType lookup table with 4 seeded types.
  Full Angular web module (list/form/detail) at `/preseller/pre-orders`. Full Flutter mobile module with
  my-orders list, create form with stock visibility, and detail screen. `Preseller` role with dedicated
  permissions. 13 integration tests. See [[2026-06-21]].
- **Preseller JWT fix** ✅ — `IdentityService.ToAuthUserAsync` now resolves `EmployeeId` from the Employee
  table when `ApplicationUser.EmployeeId` is null. JWT includes `employee_id` claim for preseller users.
  `GetMyPreOrdersQuery` now correctly returns preseller's own pre-orders.
- **Pre-order DateTime fix** ✅ — `CreatePreOrderCommandHandler` and `UpdatePreOrderCommandHandler` use
  `DateTime.SpecifyKind(..., DateTimeKind.Utc)` for `ExpectedDateUtc` to avoid PostgreSQL error.
- **Pre-order CreatedAt column** ✅ — Added `createdAt` column with `dd.MM.yyyy HH:mm` format to pre-order list grid.
- **Transfer form layout** ✅ — Source/destination warehouses now appear side-by-side under "Склад-источник → Склад-назначение" group.
- **Purchase-order form** ✅ — Currency changed from textbox to selectbox (KGS/USD/EUR/RUB/KZT/CNY/TRY). Line item fields clarified with proper placeholders.
- **Vehicle records edit/delete** ✅ — Maintenance, insurance, and inspection lists now support inline edit and delete via popup forms.
- **Category hierarchy** ✅ — `ItemCategory` now has `ParentId`/`Parent`/`Children`. Migration `AddCategoryHierarchy` applied.
  Category form includes parent category dropdown. DTO and commands updated.
- **Reports navigation** ✅ — Stock levels report has navigation links to inventory item detail pages.
- **Customer GPS & map** ✅ — Customer entity has `Latitude`/`Longitude`. MapPicker on edit form (editable marker) and detail page (non-editable).
- **Mobile preseller module** ✅ — Flutter screens: PreOrderList, PreOrderForm (customer/warehouse/payment-type
  dropdowns, item picker with stock diff, date picker, save), PreOrderDetail (status chip, submit button).
  Dashboard tile "Предзаказы" enabled. See [[2026-06-21]].

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
- Dispatchers should be able creating invoices from web app, it should create a printeable invoice which can be printed and signed by customer, printers are old dot based printers, used by warehouse workers, invoices should be designeable in separate module using devexpress report designer
- Предзаказы: CreatedAt should be shown in grid in date and time format, currently there is no such column
- Filtering across all the grids on all columns (odata can be used)
- categories should be multilevel (treeview should be used when creating stock item to select category)
- Conversion page for units-of-measures, so in reports that can be used later (ml to liter for example), because external xmls may contain different measures
- stock-operations/picking/new - currently Ячейка is free text, there should be a separate management page for Ячейка inside warehouse i think? and it should be searchable or selectable from dropdownlist when picking is being added
- transfer/new : source and destination warehouses should be in same column
use this design approach when adding positions: adjustments/new in pages like: transfer/new, packing/new, picking/new, receiving/new, purchase-orders/new
- purchase-orders/new: when adding positions it is not clear which texbox is for what, wuantity and price I guess, then also there should be a currency dropdownlist
- currency management page should be added with exchange rates management
- full e2e tests for page: personnel/attendance
- vehicles/maintenance, vehicles/insurance, vehicles/inspections: user should be able to edit/delete from these screens
- there should be a way to see all inventories of route detail with filtering, ability to navigate to related details like warehouse, inventory-detail etc, currently only orders are visible
- in customers/customers there should be a way to see all customers in map together, a new link with popup can be added, inside map also geofences should be shown as overlays(if tenant does not have acivated dispatch module, geofences should not be shown)
- from reports there should be a way to navigate to related pages

## Backlog — UI & Feature Enhancements

- **Filtering across all grids (OData)** — Add server-side filtering on all columns across all data grids.
- **Units-of-measure conversion page** — Manage conversions (ml → liter, etc.) for reports. Needed for external XML imports with different measures.
- **Bin/Location (Ячейка) management** — Entity exists. Needs: CRUD controller, frontend management page under warehouse, searchable dropdown in picking form.
- **"Add positions" design standardization** — Apply consistent position-adding pattern from `adjustments/new` to: `transfer/new`, `packing/new`, `picking/new`, `receiving/new`, `purchase-orders/new`.
- **Currency & exchange rates management page** — Manage currencies with exchange rates. Receipts should allow selecting currency and exchange rate.
- **Personnel attendance E2E tests** — Full integration test suite for `personnel/attendance` page.
- **Route detail inventory visibility** — Show all inventory items on route detail with filtering, navigation to warehouse/inventory-detail. Currently only orders visible.
- **Customers map view** — Show all customers on a single map. Popup with link. Geofence overlays (if dispatch module active).

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
