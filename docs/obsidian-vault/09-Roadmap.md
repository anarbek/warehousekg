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
- **Invoice System Phase 1** ✅ — Full backend + frontend + PDF: `Invoice` + `InvoiceLine` entities with `InvoiceType`/`InvoiceStatus`
  enums. CQRS: Create, Update, Issue, Print, Sign, Cancel, Delete, GetById, GetList with filters. `InvoicesController`
  at `/api/v1/invoices`. Auto-creation when delivery stop is completed. Sequential numbering `INV-{YEAR}-{NNNN}`.
  Angular module: list (with filters + grand total column), detail (workflow buttons + column sums), create/edit form
  (live totals, tax % input, auto-price). PDF endpoint with HTML template, column sums in `<tfoot>`, authenticated
  blob download. 10 integration tests + 4 E2E scenarios. See [[2026-06-22]].

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
- ~~Dispatchers should be able creating invoices from web app~~ → see **Invoice System** detailed plan below
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
- [x]Sales invoice(satis fatura) completed, 
- Purchase invoice(Alis fatura) should be done
- invoices module in flutter app should be done

---

## Invoice System (Detailed Plan)

> *Updated 2026-06-22 — Phase 1 implemented.*

### Phase 1 — Sales Invoice Core (Priority: HIGH) ✅ COMPLETED (2026-06-22)

**Goal**: Auto-generate a printable invoice when a delivery stop is completed. Office prints it; driver presents to customer for signature.

#### 1a. Domain & Database ✅
- [x] Add `Invoice`, `InvoiceLine` entities + `InvoiceType`, `InvoiceStatus` enums
- [x] EF Core configuration + migration (`AddInvoiceEntity`)
- [x] Sequential invoice numbering (`INV-{YEAR}-{NNNN}`)

#### 1b. Backend — CQRS ✅
- [x] `CreateInvoiceCommand` — generates invoice with sequential number
- [x] `IssueInvoiceCommand` — transitions Draft → Issued
- [x] `PrintInvoiceCommand` — marks as Printed, records `PrintedBy` + `PrintedAtUtc`
- [x] `SignInvoiceCommand` — marks as Signed (with signer name)
- [x] `CancelInvoiceCommand` — transitions to Cancelled (only Issued/Printed, not Signed)
- [x] `GetInvoiceByIdQuery` — full invoice with lines, customer, sales order ref
- [x] `GetInvoicesQuery` — paginated list with filters (status, customer, date range, warehouse)

#### 1c. Backend — Controller ✅
- [x] `InvoicesController` at `/api/v1/invoices` with all CRUD + workflow + PDF endpoints
- [x] Policies: `invoices:read`, `invoices:write`, `invoices:delete`
- [x] Resource registered in `Resources.All` for auto-policy generation
- [x] PDF endpoint: `GET /api/v1/invoices/{id}/pdf` — HTML template with column sums in `<tfoot>`

#### 1d. Integration Point ✅
- [x] `CompleteDeliveryStopCommandHandler` auto-creates `Invoice` (Issued status) when a stop with shipments is completed
- [x] Invoices are generated from the shipped `SalesOrder` lines

#### 1e. Frontend — Angular Module ✅
- [x] List: data grid with status badges, filters (status + date range), edit button for Draft, delete with permission check, **grand total column** (`totalAmount + taxAmount`)
- [x] Detail: full info with workflow buttons (Issue/Print/Sign/Cancel), PDF print via authenticated blob download, edit button for Draft, **grid with column sums** (Qty/Total/Tax/RowTotal)
- [x] Form: create/edit with customer/warehouse/item dropdowns, currency select, payment type dropdown, live computed totals (signal-based), auto-fill price on item select, select-all on focus, **sum row under columns**
- [x] SO detail → Invoice navigation: "Счета" button linking to `/invoices/list?salesOrderId=`
- [x] Nav: "Счета" under "Клиенты и поставщики" sidebar group

#### 1f. Integration Tests ✅ (10 tests)
- [x] `CreateInvoice_ReturnsId_AndAppearsInList`
- [x] `GetInvoiceById_ReturnsFullDetail_WithLines`
- [x] `GetInvoiceById_NotFound_Returns404`
- [x] `DeleteInvoice_RemovesFromList`
- [x] `InvoiceWorkflow_DraftToSigned`
- [x] `InvoiceWorkflow_DraftToCancelled`
- [x] `CancelInvoice_CannotCancelSigned`
- [x] `IssueInvoice_CannotIssueTwice`
- [x] `GetInvoices_FilterByStatus_ReturnsMatchingOnly`
- [x] `Unauthenticated_CannotAccess_Invoices`

#### 1g. E2E Tests ✅ (4 scenarios)
- [x] List invoices (filter, status, grand total column)
- [x] Create invoice with multiple lines, verify numbering, live totals, tax calculation (12 → 12%)
- [x] Edit invoice: update lines in-place, verify totals recalculate
- [x] Workflow: Draft → Issue → Print → Sign (with signer name verification) + error cases (double issue, cancel signed)
- [x] PDF: authenticated download, column sums in `<tfoot>`, tax rate display
- [x] Column consistency: same column order + sums across create/edit/detail/PDF

### Domain Context

Two distinct invoice types exist in warehouse distribution:

| | **Sales Invoice** (Outbound) | **Purchase Invoice** (Inbound) |
|---|---|---|
| **Direction** | Warehouse → Customer | Producer → Warehouse |
| **Trigger** | Delivery stop completed / SalesOrder shipped | Goods received from producer |


| **Created by** | WarehouseKG backend (auto-generated) | Parsed from producer XML |
| **Existing entity** | `SalesOrder` (Draft→Confirmed→Shipped) | `PurchaseOrder` / `StockReceipt` |
| **Goal** | Replace producer-system dependency; own invoices | Import CCI/EFS/Pepsi/Philip Morris XML formats |
| **Priority** | **Phase 1** — core feature | **Phase 5+** — depends on XML import module |

### Entity Design

New `Invoice` entity in `WarehouseKG.Domain`:

```csharp
public class Invoice : BaseEntity
{
    public string Number { get; set; } = string.Empty;      // Sequential per-tenant, e.g. "INV-2026-0001"
    public InvoiceType Type { get; set; }                    // Sales, Purchase, CreditNote
    public InvoiceStatus Status { get; set; }                // Draft, Issued, Printed, Signed, Cancelled

    // Links
    public Guid? SalesOrderId { get; set; }                  // FK → SalesOrder (for sales invoices)
    public SalesOrder? SalesOrder { get; set; }
    public Guid? PurchaseOrderId { get; set; }               // FK → PurchaseOrder (for purchase invoices)
    public PurchaseOrder? PurchaseOrder { get; set; }
    public Guid CustomerId { get; set; }                     // Denormalized for fast lookup
    public Customer? Customer { get; set; }
    public Guid? SupplierId { get; set; }                    // For purchase invoices
    public Supplier? Supplier { get; set; }
    public Guid WarehouseId { get; set; }                    // Originating warehouse
    public Warehouse? Warehouse { get; set; }

    // Dates
    public DateTime IssuedAtUtc { get; set; }
    public DateTime? PrintedAtUtc { get; set; }
    public DateTime? SignedAtUtc { get; set; }
    public DateTime? DueDateUtc { get; set; }                // Payment due date

    // Financial
    public decimal TotalAmount { get; set; }                 // Computed from order lines
    public decimal TaxAmount { get; set; }
    public string Currency { get; set; } = "KGS";
    public decimal ExchangeRate { get; set; } = 1m;          // Rate to base currency
    public string? PaymentType { get; set; }                 // Cash, BankTransfer, etc.

    // Print & Signature
    public string? ReportLayoutId { get; set; }               // DevExpress report template ID
    public string? PrintedBy { get; set; }                    // User who printed
    public string? SignedByName { get; set; }                 // Customer signature name
    public string? SignatureDataUrl { get; set; }             // Base64 signature image (mobile capture)

    // Metadata
    public string? Notes { get; set; }
    public string? ExternalReference { get; set; }            // Producer's invoice number (XML import)

    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
}

public class InvoiceLine : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public Guid InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }                    // Quantity × UnitPrice
    public decimal TaxRate { get; set; }                      // e.g. 0.12 for 12% VAT
    public decimal TaxAmount { get; set; }
    public string? Notes { get; set; }
}

public enum InvoiceType { Sales = 0, Purchase = 1, CreditNote = 2 }
public enum InvoiceStatus { Draft = 0, Issued = 1, Printed = 2, Signed = 3, Cancelled = 4 }
```

**Indexes**: `(TenantId, Number)` unique, `(TenantId, CustomerId)`, `(TenantId, SalesOrderId)`, `(TenantId, Status)`.

### Phase 1 — Sales Invoice Core (Priority: HIGH)

**Goal**: Auto-generate a printable invoice when a delivery stop is completed. Office prints it; driver presents to customer for signature.

#### 1a. Domain & Database
- [ ] Add `Invoice`, `InvoiceLine` entities + `InvoiceType`, `InvoiceStatus` enums
- [ ] EF Core configuration + migration
- [ ] Seed data: invoice numbering sequence per tenant (e.g., `InvoiceSequence` table or `Tenant.NextInvoiceNumber`)

#### 1b. Backend — CQRS
- [ ] `CreateInvoiceCommand` — generates invoice from `SalesOrder`; sets `Number` from sequence
- [ ] `IssueInvoiceCommand` — transitions Draft → Issued
- [ ] `PrintInvoiceCommand` — marks as Printed, records `PrintedBy` + `PrintedAtUtc`
- [ ] `SignInvoiceCommand` — marks as Signed (for mobile signature capture)
- [ ] `CancelInvoiceCommand` — transitions to Cancelled (only Issued/Printed, not Signed)
- [ ] `GetInvoiceByIdQuery` — full invoice with lines, customer, sales order ref
- [ ] `GetInvoicesQuery` — paginated list with filters (status, customer, date range, warehouse)
- [ ] `GetInvoicePdfQuery` — returns PDF bytes (delegates to report engine)

**Integration point**: Modify `CompleteDeliveryStopCommandHandler` to **auto-create** an `Invoice` (Draft → auto-Issued) when the stop is completed. This ties invoice generation to the natural delivery workflow.

#### 1c. Backend — Controller ✅
- [x] `InvoicesController` at `/api/v1/invoices`
- [x] Policies: `invoices:read`, `invoices:write`, `invoices:delete` (auto-generated from `Resources.All`)
- [x] PDF download endpoint: `GET /api/v1/invoices/{id}/pdf` — returns HTML invoice (report engine placeholder)
- [x] Permissions registered in `TenantPermission` seed (Dispatcher: read+write+delete; Driver: read)

#### 1d. Frontend — Angular ✅
- [x] `InvoiceListComponent` at `/invoices/list` — DevExtreme data grid with filters (status, date range), status badges, delete with permission check
- [x] `InvoiceDetailComponent` at `/invoices/:id` — full invoice view, customer info, line items, totals, tax breakdown, status badge, workflow buttons
- [x] Invoice form at `/invoices/new` — customer/warehouse dropdowns, currency select, payment type dropdown, date picker, labeled line items with auto-fill price on item select, select-all on textbox focus
- [x] Print button → opens `GET /api/v1/invoices/{id}/pdf` in new tab, auto-marks as Printed
- [x] "Print & Sign" workflow: Draft → Issue → Print → Sign (with signer name)
- [x] Navigation links from SalesOrder detail → related Invoice (via `salesOrderId` query param)
- [x] `salesOrderId` filter in `GetInvoicesQuery` for cross-entity navigation

#### 1e. Mobile — Flutter

- [ ] `InvoiceViewScreen` — view invoice PDF (via `flutter_pdfview` or webview)
- [ ] Print trigger — send to Bluetooth/network printer (`flutter_bluetooth_serial` or raw socket to dot-matrix printer)
- [ ] Signature capture — draw signature on canvas, upload via `SignInvoiceCommand`
- [ ] Invoice list on driver's route stop screen — see invoices for shipments at current stop
- [ ] After stop completion, show "Invoice ready" with print/view options

### Phase 2 — Invoice Numbering & Sequences
- [ ] `InvoiceSequence` entity: per-tenant, per-type numbering with prefix, padding
- [ ] Configurable format: `{PREFIX}-{YEAR}-{NNNN}`, `{NNNNNN}`, etc.
- [ ] Tenant settings UI for invoice numbering format
- [ ] Concurrency-safe number allocation (database sequence or row lock)

### Phase 3 — Printable Layout (DevExpress Report Designer)
- [ ] Design report template in DevExpress Report Designer
- [ ] Multiple templates: A4 laser (modern), dot-matrix 80mm (legacy), thermal 58mm
- [ ] Template variables: invoice number, date, customer name/address/taxId, line items, totals, warehouse stamp
- [ ] `ReportLayout` entity — tenants can upload/select custom layouts
- [ ] PDF rendering endpoint uses selected layout
- [ ] Print preview in Angular with DevExpress report viewer component

### Phase 4 — Credit Notes & Corrections
- [ ] `CreditNote` → subtype of `Invoice` with `InvoiceType.CreditNote`
- [ ] Link to original `Invoice` via `OriginalInvoiceId`
- [ ] Workflow: Cancelled invoice → optional credit note for accounting
- [ ] Re-issue capability: cancelled invoices can trigger a new Draft

### Phase 5 — Purchase Invoice XML Import
- [ ] XML parser abstraction with format-specific implementations
- [ ] CCI format parser (Coca-Cola)
- [ ] EFS format parser
- [ ] Pepsi format parser
- [ ] Philip Morris format parser
- [ ] Import endpoint: `POST /api/v1/invoices/import-xml` — parses XML, creates `PurchaseOrder` + `Invoice`
- [ ] Validation: duplicate detection via `ExternalReference`, schema validation
- [ ] Dashboard for import history with success/error logs

### Phase 6 — Invoice Reports & Analytics
- [ ] Invoice summary report: issued, printed, signed, cancelled counts per period
- [ ] Revenue by customer/warehouse/period
- [ ] Outstanding (unpaid) invoices aging report
- [ ] Tax summary report (for fiscal authorities)
- [ ] Export to Excel/CSV

### Auth & Permissions

| Permission | Role |
|---|---|
| `invoices:read` | Admin, Dispatcher, Driver, Preseller |
| `invoices:write` | Admin, Dispatcher |
| `invoices:delete` | Admin |
| `invoices:print` | Admin, Dispatcher, Driver |
| `invoices:sign` | Admin, Dispatcher, Driver |

---

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
