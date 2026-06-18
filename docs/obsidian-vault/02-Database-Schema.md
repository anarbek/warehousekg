# Database Schema

Entities, relationships, tables, and migrations for the data layer.

## Multitenancy & BaseEntity

All persistent domain entities inherit from `BaseEntity`, which provides:

| Column     | Type   | Notes                                  |
| ---------- | ------ | -------------------------------------- |
| `Id`       | `Guid` | Primary key                            |
| `TenantId` | `Guid` | Owning tenant (discriminator column)   |

The system uses a **shared database with a `TenantId` discriminator** rather than schema-per-tenant. Every tenant-owned table carries a `TenantId` column.

- A **global query filter** (`WHERE TenantId = @currentTenant`) is applied to all `BaseEntity`-derived tables, enforcing row-level isolation transparently on every query.
- `TenantId` is set automatically on insert from the current tenant context, so application code never sets it manually.
- The current tenant is resolved per request via `ITenantProvider` (HTTP `X-Tenant-Id` header for now). See [[01-Architecture]] for the resolution flow.

> Indexing note: include `TenantId` as the leading column in composite indexes on tenant-owned tables to keep tenant-scoped queries efficient.

## Inventory Module

The first domain module. All tables below inherit `Id` + `TenantId` from `BaseEntity`. Migration: `InitialInventory`.

### `warehouses`
Physical warehouses owned by a tenant.

| Column     | Type            | Notes                          |
| ---------- | --------------- | ------------------------------ |
| `Id`       | `uuid`          | PK                             |
| `Code`     | `varchar(32)`   | Required; unique per tenant    |
| `Name`     | `varchar(256)`  | Required                       |
| `Address`  | `varchar(512)`  | Optional                       |
| `IsActive` | `boolean`       |                                |
| `TenantId` | `uuid`          | Tenant discriminator           |

Unique index: `(TenantId, Code)`.

### `warehouse_locations`
Zones / aisles / bins within a warehouse.

| Column        | Type           | Notes                                   |
| ------------- | -------------- | --------------------------------------- |
| `Id`          | `uuid`         | PK                                      |
| `WarehouseId` | `uuid`         | FK → `warehouses` (cascade delete)      |
| `Code`        | `varchar(64)`  | Required; unique per warehouse + tenant |
| `Zone`        | `varchar(64)`  | Optional                                |
| `Aisle`       | `varchar(64)`  | Optional                                |
| `Bin`         | `varchar(64)`  | Optional                                |
| `IsActive`    | `boolean`      |                                         |
| `TenantId`    | `uuid`         | Tenant discriminator                    |

Unique index: `(TenantId, WarehouseId, Code)`.

### `item_categories`
Classification of inventory items.

| Column        | Type           | Notes                       |
| ------------- | -------------- | --------------------------- |
| `Id`          | `uuid`         | PK                          |
| `Code`        | `varchar(32)`  | Required; unique per tenant |
| `Name`        | `varchar(256)` | Required                    |
| `Description` | `varchar(512)` | Optional                    |
| `TenantId`    | `uuid`         | Tenant discriminator        |

Unique index: `(TenantId, Code)`.

### `units_of_measure`
Units such as pcs, kg, l.

| Column        | Type           | Notes                       |
| ------------- | -------------- | --------------------------- |
| `Id`          | `uuid`         | PK                          |
| `Code`        | `varchar(16)`  | Required; unique per tenant |
| `Name`        | `varchar(128)` | Required                    |
| `Description` | `varchar(512)` | Optional                    |
| `TenantId`    | `uuid`         | Tenant discriminator        |

Unique index: `(TenantId, Code)`.

### `inventory_items`
The catalog of stock-keeping items.

| Column            | Type             | Notes                                       |
| ----------------- | ---------------- | ------------------------------------------- |
| `Id`              | `uuid`           | PK                                          |
| `Sku`             | `varchar(64)`    | Required; unique per tenant                 |
| `Name`            | `varchar(256)`   | Required                                    |
| `Description`     | `varchar(1024)`  | Optional                                    |
| `Barcode`         | `varchar(128)`   | Optional; indexed per tenant                |
| `CategoryId`      | `uuid`           | FK → `item_categories` (restrict delete)    |
| `UnitOfMeasureId` | `uuid`           | FK → `units_of_measure` (restrict delete)   |
| `QuantityOnHand`  | `numeric(18,3)`  | Current stock                               |
| `ReorderLevel`    | `numeric(18,3)`  | Low-stock threshold                         |
| `IsActive`        | `boolean`        |                                             |
| `TenantId`        | `uuid`           | Tenant discriminator                        |

Indexes: unique `(TenantId, Sku)`, non-unique `(TenantId, Barcode)`.

### Relationships

```
Warehouse 1───* WarehouseLocation
ItemCategory 1───* InventoryItem
UnitOfMeasure 1───* InventoryItem
```

See [[03-API-Endpoints]] for the endpoints exposing these entities.

## Stock Operations Module

Document-style operations that move inventory. Migration: `AddStockOperations`. Each operation has a
header + lines (both inherit `Id` + `TenantId` from `BaseEntity`) and a `Status` stored as text
(`Draft`, `Completed`, `Cancelled`). Header `Number` is unique per tenant: `(TenantId, Number)`.

| Operation       | Header table        | Line table               | Stock effect on **Complete**                    |
| --------------- | ------------------- | ------------------------ | ----------------------------------------------- |
| Receiving       | `stock_receipts`    | `stock_receipt_lines`    | `QuantityOnHand += line.Quantity`               |
| Picking         | `pick_orders`       | `pick_order_lines`       | `QuantityOnHand -= line.Quantity`               |
| Packing         | `pack_orders`       | `pack_order_lines`       | none (status only)                              |
| Transfers       | `stock_transfers`   | `stock_transfer_lines`   | none (status only)                              |
| Adjustment      | `stock_adjustments` | `stock_adjustment_lines` | `QuantityOnHand += line.QuantityChange`         |
| Audit           | `stock_audits`      | `stock_audit_lines`      | `QuantityOnHand += (counted − system)`          |
| Purchase (recv) | `purchase_orders`   | `purchase_order_lines`   | `QuantityOnHand += line.Quantity`               |
| Sales (ship)    | `sales_orders`      | `sales_order_lines`      | `QuantityOnHand -= line.Quantity`               |

### Headers

- **`stock_receipts`** — `Number`, `WarehouseId` (FK → `warehouses`, restrict), `SupplierReference`, `Status`, `ReceivedAtUtc`, `Notes`.
- **`pick_orders`** — `Number`, `WarehouseId` (FK → `warehouses`, restrict), `Reference`, `Status`, `PickedAtUtc`, `Notes`.
- **`pack_orders`** — `Number`, `WarehouseId` (FK → `warehouses`, restrict), `PickOrderId` (FK → `pick_orders`, restrict, optional), `Status`, `PackedAtUtc`, `Notes`.
- **`stock_transfers`** — `Number`, `SourceWarehouseId` + `DestinationWarehouseId` (both FK → `warehouses`, restrict), `Status`, `TransferredAtUtc`, `Notes`.

### Lines

Each line carries `Quantity numeric(18,3)`, a restrict FK to `inventory_items`, and a cascade FK to its
header (deleting a header removes its lines). Receipt/pick lines also have an optional restrict FK to
`warehouse_locations`; pack lines add an optional `PackageLabel`.

### Workflow

Operations are created as `Draft`. `Complete` and `Cancel` are only valid from `Draft` (otherwise the
API returns `409 Conflict`). Completing applies the stock effect in the table above.

> **Note:** `InventoryItem.QuantityOnHand` is a single tenant-wide figure, maintained as the sum of all
> completed operation deltas across all warehouses. The `warehouse-stock` report endpoint computes per-warehouse
> balances by replaying operations filtered by warehouse — so Оборот (turnover, excluding audit deltas) and
> Всего (ending balance, including all deltas) are always consistent with the item movement history for that
> warehouse. Per-location stock tracking (`StockLevel`) is a planned enhancement (see [[09-Roadmap]]).

## Suppliers & Purchase Orders Module

Migration: `AddSuppliersAndPurchaseOrders`. All tables inherit `Id` + `TenantId` from `BaseEntity`.

### `suppliers`
Vendors that goods are purchased from.

| Column        | Type           | Notes                       |
| ------------- | -------------- | --------------------------- |
| `Id`          | `uuid`         | PK                          |
| `Code`        | `varchar(32)`  | Required; unique per tenant |
| `Name`        | `varchar(256)` | Required                    |
| `ContactName` | `varchar(256)` | Optional                    |
| `Email`       | `varchar(256)` | Optional                    |
| `Phone`       | `varchar(64)`  | Optional                    |
| `Address`     | `varchar(512)` | Optional                    |
| `TaxId`       | `varchar(64)`  | Optional                    |
| `IsActive`    | `boolean`      |                             |
| `TenantId`    | `uuid`         | Tenant discriminator        |

Unique index: `(TenantId, Code)`.

### `purchase_orders`
Order headers placed with a supplier. `Status` stored as text (`Draft`, `Submitted`, `Received`, `Cancelled`).

| Column            | Type           | Notes                                        |
| ----------------- | -------------- | -------------------------------------------- |
| `Id`              | `uuid`         | PK                                           |
| `Number`          | `varchar(64)`  | Required; unique per tenant                  |
| `SupplierId`      | `uuid`         | FK → `suppliers` (restrict)                  |
| `WarehouseId`     | `uuid?`        | FK → `warehouses` (restrict, optional dest.) |
| `Status`          | `varchar(32)`  | `Draft` / `Submitted` / `Received` / `Cancelled` |
| `Currency`        | `varchar(3)`   | Default `KGS`                                |
| `OrderDateUtc`    | `timestamp`    | Set on creation                              |
| `ExpectedDateUtc` | `timestamp?`   | Optional                                     |
| `SubmittedAtUtc`  | `timestamp?`   | Set on submit                                |
| `ReceivedAtUtc`   | `timestamp?`   | Set on receive                               |
| `Notes`           | `varchar(1024)`| Optional                                     |
| `TenantId`        | `uuid`         | Tenant discriminator                         |

Unique index: `(TenantId, Number)`.

### `purchase_order_lines`

| Column            | Type            | Notes                                  |
| ----------------- | --------------- | -------------------------------------- |
| `Id`              | `uuid`          | PK                                     |
| `PurchaseOrderId` | `uuid`          | FK → `purchase_orders` (cascade)       |
| `InventoryItemId` | `uuid`          | FK → `inventory_items` (restrict)      |
| `Quantity`        | `numeric(18,3)` |                                        |
| `UnitPrice`       | `numeric(18,2)` |                                        |
| `TenantId`        | `uuid`          | Tenant discriminator                   |

`TotalAmount` / `LineTotal` are computed (`Quantity × UnitPrice`) in the read models, not stored.

### Relationships & workflow

```
Supplier 1───* PurchaseOrder 1───* PurchaseOrderLine
PurchaseOrder *───1 Warehouse (optional destination)
```

Orders start `Draft`. **Submit** (`Draft → Submitted`), **Receive** (`Submitted → Received`, increases
`InventoryItem.QuantityOnHand` by line quantities), **Cancel** (`Draft`/`Submitted → Cancelled`). Invalid
transitions return `409 Conflict`. See [[03-API-Endpoints]].

## Customers & Sales Orders Module

Migration: `AddCustomersAndSalesOrders`. The sell-side mirror of suppliers/purchase orders. All tables
inherit `Id` + `TenantId` from `BaseEntity`.

### `customers`
End customers that goods are sold to.

| Column        | Type           | Notes                       |
| ------------- | -------------- | --------------------------- |
| `Id`          | `uuid`         | PK                          |
| `Code`        | `varchar(32)`  | Required; unique per tenant |
| `Name`        | `varchar(256)` | Required                    |
| `ContactName` | `varchar(256)` | Optional                    |
| `Email`       | `varchar(256)` | Optional                    |
| `Phone`       | `varchar(64)`  | Optional                    |
| `Address`     | `varchar(512)` | Optional                    |
| `TaxId`       | `varchar(64)`  | Optional                    |
| `IsActive`    | `boolean`      |                             |
| `TenantId`    | `uuid`         | Tenant discriminator        |

Unique index: `(TenantId, Code)`.

### `sales_orders`
Order headers placed by a customer. `Status` stored as text (`Draft`, `Confirmed`, `Shipped`, `Cancelled`).

| Column            | Type           | Notes                                      |
| ----------------- | -------------- | ------------------------------------------ |
| `Id`              | `uuid`         | PK                                         |
| `Number`          | `varchar(64)`  | Required; unique per tenant                |
| `CustomerId`      | `uuid`         | FK → `customers` (restrict)                |
| `WarehouseId`     | `uuid?`        | FK → `warehouses` (restrict, optional src.)|
| `Status`          | `varchar(32)`  | `Draft` / `Confirmed` / `Shipped` / `Cancelled` |
| `Currency`        | `varchar(3)`   | Default `KGS`                              |
| `OrderDateUtc`    | `timestamp`    | Set on creation                            |
| `ExpectedDateUtc` | `timestamp?`   | Optional                                   |
| `ConfirmedAtUtc`  | `timestamp?`   | Set on confirm                             |
| `ShippedAtUtc`    | `timestamp?`   | Set on ship                                |
| `Notes`           | `varchar(1024)`| Optional                                   |
| `TenantId`        | `uuid`         | Tenant discriminator                       |

Unique index: `(TenantId, Number)`.

### `sales_order_lines`

| Column            | Type            | Notes                                  |
| ----------------- | --------------- | -------------------------------------- |
| `Id`              | `uuid`          | PK                                     |
| `SalesOrderId`    | `uuid`          | FK → `sales_orders` (cascade)          |
| `InventoryItemId` | `uuid`          | FK → `inventory_items` (restrict)      |
| `Quantity`        | `numeric(18,3)` |                                        |
| `UnitPrice`       | `numeric(18,2)` |                                        |
| `TenantId`        | `uuid`          | Tenant discriminator                   |

`TotalAmount` / `LineTotal` are computed (`Quantity × UnitPrice`) in the read models, not stored.

### Relationships & workflow

```
Customer 1───* SalesOrder 1───* SalesOrderLine
SalesOrder *───1 Warehouse (optional source)
```

Orders start `Draft`. **Confirm** (`Draft → Confirmed`), **Ship** (`Confirmed → Shipped`, decreases
`InventoryItem.QuantityOnHand` by line quantities), **Cancel** (`Draft`/`Confirmed → Cancelled`). Invalid
transitions return `409 Conflict`. See [[03-API-Endpoints]].

## Stock Adjustments & Audits Module

Migration: `AddStockAdjustmentsAndAudits`. Two document-style operations for keeping the catalog's
`QuantityOnHand` in sync with reality. Both inherit `Id` + `TenantId` from `BaseEntity`, store `Status`
as text (`Draft`, `Completed`, `Cancelled`), and have a `Number` that is unique per tenant: `(TenantId, Number)`.

### `stock_adjustments`
Manual stock corrections (damage, loss, theft, found, expiry, etc.).

| Column          | Type            | Notes                                                       |
| --------------- | --------------- | ----------------------------------------------------------- |
| `Id`            | `uuid`          | PK                                                          |
| `Number`        | `varchar(64)`   | Required; unique per tenant                                 |
| `WarehouseId`   | `uuid`          | FK → `warehouses` (restrict)                                |
| `Reason`        | `varchar(32)`   | `Correction` / `Damage` / `Loss` / `Theft` / `Found` / `Expired` / `Other` |
| `Status`        | `varchar(32)`   | `Draft` / `Completed` / `Cancelled`                         |
| `AdjustedAtUtc` | `timestamp?`    | Set when completed                                          |
| `Notes`         | `varchar(1024)` | Optional                                                    |
| `TenantId`      | `uuid`          | Tenant discriminator                                        |

Unique index: `(TenantId, Number)`.

### `stock_adjustment_lines`

| Column              | Type            | Notes                                              |
| ------------------- | --------------- | -------------------------------------------------- |
| `Id`                | `uuid`          | PK                                                 |
| `StockAdjustmentId` | `uuid`          | FK → `stock_adjustments` (cascade)                 |
| `InventoryItemId`   | `uuid`          | FK → `inventory_items` (restrict)                  |
| `QuantityChange`    | `numeric(18,3)` | **Signed** delta (positive adds, negative removes) |
| `Notes`             | `varchar(512)`  | Optional                                           |
| `TenantId`          | `uuid`          | Tenant discriminator                               |

### `stock_audits`
Physical counts / stocktakes that reconcile the system to what is actually on the shelf.

| Column            | Type            | Notes                               |
| ----------------- | --------------- | ----------------------------------- |
| `Id`              | `uuid`          | PK                                  |
| `Number`          | `varchar(64)`   | Required; unique per tenant         |
| `WarehouseId`     | `uuid`          | FK → `warehouses` (restrict)        |
| `Status`          | `varchar(32)`   | `Draft` / `Completed` / `Cancelled` |
| `ReconciledAtUtc` | `timestamp?`    | Set when completed                  |
| `Notes`           | `varchar(1024)` | Optional                            |
| `TenantId`        | `uuid`          | Tenant discriminator                |

Unique index: `(TenantId, Number)`.

### `stock_audit_lines`

| Column            | Type            | Notes                                                       |
| ----------------- | --------------- | ----------------------------------------------------------- |
| `Id`              | `uuid`          | PK                                                          |
| `StockAuditId`    | `uuid`          | FK → `stock_audits` (cascade)                               |
| `InventoryItemId` | `uuid`          | FK → `inventory_items` (restrict)                           |
| `SystemQuantity`  | `numeric(18,3)` | Book quantity snapshotted when the line is created          |
| `CountedQuantity` | `numeric(18,3)` | Physically counted quantity                                 |
| `TenantId`        | `uuid`          | Tenant discriminator                                        |

`Variance` (`CountedQuantity − SystemQuantity`) is computed in the read models, not stored.

### Relationships & workflow

```
Warehouse 1───* StockAdjustment 1───* StockAdjustmentLine
Warehouse 1───* StockAudit      1───* StockAuditLine
```

Both start `Draft`. **Complete** and **Cancel** are only valid from `Draft` (otherwise `409 Conflict`):

- **Adjustment complete** — applies each line's signed `QuantityChange` to `InventoryItem.QuantityOnHand`.
- **Audit complete** — applies the variance of each line (`CountedQuantity − SystemQuantity`) to `InventoryItem.QuantityOnHand`, preserving adjustments made at other warehouses since the audit was created.

> Like the rest of the system these adjust the single tenant-wide `InventoryItem.QuantityOnHand`. Per-location
> reconciliation is part of the planned `StockLevel` work (see [[09-Roadmap]]).

See [[03-API-Endpoints]].

## Reporting Module

The reporting endpoints (`/api/v1/reports`, see [[03-API-Endpoints]]) are **read-only** and introduce
**no new tables, entities, or migrations**. Each report is a MediatR query that aggregates over the
existing tables above (`inventory_items`, `sales_orders`, `purchase_orders`, and the stock-operation
tables), respecting the tenant query filter like every other read. As data volumes grow, candidates for
optimization include database views or pre-aggregated summary tables — tracked in [[09-Roadmap]].
