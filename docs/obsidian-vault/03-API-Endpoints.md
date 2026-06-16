# API Endpoints

Reference of REST/RPC endpoints: routes, request/response shapes, and status codes.

## Conventions

- Base path is versioned: `/api/v1/...`.
- All requests are tenant-scoped via the `X-Tenant-Id` header (see [[01-Architecture]]). For authenticated
  callers the tenant also falls back to the JWT `tenant_id` claim (see [[04-Auth-Flow]]).
- Authentication is JWT bearer; obtain tokens from `/api/v1/auth/*` (see [[04-Auth-Flow]]). Role-based
  authorization policies are defined but not yet enforced on the domain controllers.
- Responses are `application/json`. Interactive docs are available via **Swagger UI** at `/swagger`
  (Development environment), which includes an **Authorize** button for the bearer token.

## Inventory Module

Entities are described in [[02-Database-Schema]].

> **Frontend routes (Angular)** — The `InventoryModule` feature is lazy-loaded under `/inventory`:
>
> | Angular route                        | Component                |
> | ------------------------------------ | ------------------------ |
> | `/inventory/warehouses`              | `WarehouseList`          |
> | `/inventory/warehouses/new`          | `WarehouseForm` (create) |
> | `/inventory/warehouses/:id`          | `WarehouseDetail`        |
> | `/inventory/warehouses/:id/edit`     | `WarehouseForm` (edit)   |
> | `/inventory/items`                   | `InventoryItemList`      |
> | `/inventory/items/new`               | `InventoryItemForm` (create) |
> | `/inventory/items/:id`               | `InventoryItemDetail`    |
> | `/inventory/items/:id/edit`          | `InventoryItemForm` (edit)|
>
> The shared `InventoryService` (`features/inventory/services/inventory.service.ts`) wraps both
> `/api/v1/warehouses` and `/api/v1/inventory-items`.
>
> **Note on `categoryId` / `unitOfMeasureId`**: The current API has no dedicated list endpoints
> for categories or units of measure. The inventory item form therefore accepts raw UUIDs.
> When those endpoints are added, the form should be upgraded to proper `<select>` dropdowns.

### Warehouses — `/api/v1/warehouses`

| Method | Route                       | Description              | Success         | Body |
| ------ | --------------------------- | ------------------------ | --------------- | ---- |
| GET    | `/api/v1/warehouses`        | List all warehouses      | `200 OK`        | —    |
| GET    | `/api/v1/warehouses/{id}`   | Get a warehouse by id    | `200 OK` / `404`| —    |
| POST   | `/api/v1/warehouses`        | Create a warehouse       | `201 Created`   | `CreateWarehouse` |
| PUT    | `/api/v1/warehouses/{id}`   | Update a warehouse       | `204` / `404`   | `UpdateWarehouse` |
| DELETE | `/api/v1/warehouses/{id}`   | Delete a warehouse       | `204` / `404`   | —    |

**Create/Update body**

```json
{
  "code": "WH-BISH-01",
  "name": "Bishkek Central",
  "address": "ul. Chuy 1, Bishkek",
  "isActive": true
}
```

`POST` returns the new warehouse `Guid`.

### Inventory Items — `/api/v1/inventory-items`

| Method | Route                            | Description                | Success          | Body |
| ------ | -------------------------------- | -------------------------- | ---------------- | ---- |
| GET    | `/api/v1/inventory-items`        | List all inventory items   | `200 OK`         | —    |
| GET    | `/api/v1/inventory-items/{id}`   | Get an item by id          | `200 OK` / `404` | —    |
| POST   | `/api/v1/inventory-items`        | Create an inventory item   | `201 Created`    | `CreateInventoryItem` |
| PUT    | `/api/v1/inventory-items/{id}`   | Update an inventory item   | `204` / `404`    | `UpdateInventoryItem` |
| DELETE | `/api/v1/inventory-items/{id}`   | Delete an inventory item   | `204` / `404`    | —    |

**Create/Update body**

```json
{
  "sku": "SKU-1001",
  "name": "Cordless Drill",
  "description": "18V brushless drill",
  "barcode": "4690000000017",
  "categoryId": "00000000-0000-0000-0000-000000000000",
  "unitOfMeasureId": "00000000-0000-0000-0000-000000000000",
  "quantityOnHand": 25,
  "reorderLevel": 5,
  "isActive": true
}
```

`POST` returns the new inventory item `Guid`.

## Stock Operations Module

Document-style operations (see [[02-Database-Schema]]). All four follow the same shape:

- `GET /...` — list (summary rows: number, warehouse, status, line count)
- `GET /.../{id}` — full document with lines, or `404`
- `POST /...` — create a **Draft**; returns the new `Guid` (`201 Created`)
- `POST /.../{id}/complete` — transition Draft → Completed; `204`, `404`, or `409 Conflict` (not in Draft)
- `POST /.../{id}/cancel` — transition Draft → Cancelled; `204`, `404`, or `409 Conflict`

| Operation  | Base route                  | Complete stock effect            |
| ---------- | --------------------------- | -------------------------------- |
| Receiving  | `/api/v1/stock-receipts`    | increases `QuantityOnHand`       |
| Picking    | `/api/v1/pick-orders`       | decreases `QuantityOnHand`       |
| Packing    | `/api/v1/pack-orders`       | none (status only)               |
| Transfers  | `/api/v1/stock-transfers`   | none (status only)               |

### Create body examples

**Stock receipt** — `POST /api/v1/stock-receipts`

```json
{
  "number": "RCV-0001",
  "warehouseId": "00000000-0000-0000-0000-000000000000",
  "supplierReference": "PO-12345",
  "notes": "Inbound from supplier",
  "lines": [
    { "inventoryItemId": "00000000-0000-0000-0000-000000000000", "warehouseLocationId": null, "quantity": 100 }
  ]
}
```

**Pick order** — `POST /api/v1/pick-orders`

```json
{
  "number": "PICK-0001",
  "warehouseId": "00000000-0000-0000-0000-000000000000",
  "reference": "SO-9876",
  "notes": null,
  "lines": [
    { "inventoryItemId": "00000000-0000-0000-0000-000000000000", "warehouseLocationId": null, "quantity": 5 }
  ]
}
```

**Pack order** — `POST /api/v1/pack-orders`

```json
{
  "number": "PACK-0001",
  "warehouseId": "00000000-0000-0000-0000-000000000000",
  "pickOrderId": null,
  "notes": null,
  "lines": [
    { "inventoryItemId": "00000000-0000-0000-0000-000000000000", "quantity": 5, "packageLabel": "BOX-001" }
  ]
}
```

**Stock transfer** — `POST /api/v1/stock-transfers`

```json
{
  "number": "TRF-0001",
  "sourceWarehouseId": "00000000-0000-0000-0000-000000000000",
  "destinationWarehouseId": "00000000-0000-0000-0000-000000000000",
  "notes": null,
  "lines": [
    { "inventoryItemId": "00000000-0000-0000-0000-000000000000", "quantity": 10 }
  ]
}
```

## Suppliers & Purchase Orders Module

Entities described in [[02-Database-Schema]].

### Suppliers — `/api/v1/suppliers`

Standard CRUD (same shape as warehouses / inventory items).

| Method | Route                       | Description           | Success          | Body |
| ------ | --------------------------- | --------------------- | ---------------- | ---- |
| GET    | `/api/v1/suppliers`         | List suppliers        | `200 OK`         | —    |
| GET    | `/api/v1/suppliers/{id}`    | Get supplier by id    | `200 OK` / `404` | —    |
| POST   | `/api/v1/suppliers`         | Create supplier       | `201 Created`    | `CreateSupplier` |
| PUT    | `/api/v1/suppliers/{id}`    | Update supplier       | `204` / `404`    | `UpdateSupplier` |
| DELETE | `/api/v1/suppliers/{id}`    | Delete supplier       | `204` / `404`    | —    |

**Create/Update body**

```json
{
  "code": "SUP-001",
  "name": "Acme Tools LLC",
  "contactName": "Aibek",
  "email": "sales@acme.example",
  "phone": "+996700000000",
  "address": "Bishkek",
  "taxId": "1234567890",
  "isActive": true
}
```

### Purchase Orders — `/api/v1/purchase-orders`

| Method | Route                                  | Description                          | Success                |
| ------ | -------------------------------------- | ------------------------------------ | ---------------------- |
| GET    | `/api/v1/purchase-orders`              | List (summary + total)               | `200 OK`               |
| GET    | `/api/v1/purchase-orders/{id}`         | Get full order with lines            | `200 OK` / `404`       |
| POST   | `/api/v1/purchase-orders`              | Create a **Draft**                   | `201 Created`          |
| POST   | `/api/v1/purchase-orders/{id}/submit`  | Draft → Submitted                    | `204` / `404` / `409`  |
| POST   | `/api/v1/purchase-orders/{id}/receive` | Submitted → Received (+ stock on hand)| `204` / `404` / `409`  |
| POST   | `/api/v1/purchase-orders/{id}/cancel`  | Draft/Submitted → Cancelled          | `204` / `404` / `409`  |

`409 Conflict` is returned when the order is not in a state that allows the transition.

**Create body** — `POST /api/v1/purchase-orders`

```json
{
  "number": "PO-0001",
  "supplierId": "00000000-0000-0000-0000-000000000000",
  "warehouseId": "00000000-0000-0000-0000-000000000000",
  "currency": "KGS",
  "expectedDateUtc": "2026-07-01T00:00:00Z",
  "notes": "Quarterly restock",
  "lines": [
    { "inventoryItemId": "00000000-0000-0000-0000-000000000000", "quantity": 100, "unitPrice": 250.00 }
  ]
}
```

`POST` returns the new purchase order `Guid`. Read models include a computed `totalAmount` (and per-line `lineTotal`) of `quantity × unitPrice`.

## Customers & Sales Orders Module

Entities described in [[02-Database-Schema]]. The sell-side mirror of suppliers / purchase orders.

### Customers — `/api/v1/customers`

Standard CRUD.

| Method | Route                       | Description           | Success          | Body |
| ------ | --------------------------- | --------------------- | ---------------- | ---- |
| GET    | `/api/v1/customers`         | List customers        | `200 OK`         | —    |
| GET    | `/api/v1/customers/{id}`    | Get customer by id    | `200 OK` / `404` | —    |
| POST   | `/api/v1/customers`         | Create customer       | `201 Created`    | `CreateCustomer` |
| PUT    | `/api/v1/customers/{id}`    | Update customer       | `204` / `404`    | `UpdateCustomer` |
| DELETE | `/api/v1/customers/{id}`    | Delete customer       | `204` / `404`    | —    |

**Create/Update body**

```json
{
  "code": "CUST-001",
  "name": "Bishkek Retail LLC",
  "contactName": "Nurlan",
  "email": "orders@retail.example",
  "phone": "+996700111222",
  "address": "Bishkek",
  "taxId": "0987654321",
  "isActive": true
}
```

### Sales Orders — `/api/v1/sales-orders`

| Method | Route                                | Description                         | Success                |
| ------ | ------------------------------------ | ----------------------------------- | ---------------------- |
| GET    | `/api/v1/sales-orders`               | List (summary + total)              | `200 OK`               |
| GET    | `/api/v1/sales-orders/{id}`          | Get full order with lines           | `200 OK` / `404`       |
| POST   | `/api/v1/sales-orders`               | Create a **Draft**                  | `201 Created`          |
| POST   | `/api/v1/sales-orders/{id}/confirm`  | Draft → Confirmed                   | `204` / `404` / `409`  |
| POST   | `/api/v1/sales-orders/{id}/ship`     | Confirmed → Shipped (− stock on hand)| `204` / `404` / `409`  |
| POST   | `/api/v1/sales-orders/{id}/cancel`   | Draft/Confirmed → Cancelled         | `204` / `404` / `409`  |

`409 Conflict` is returned when the order is not in a state that allows the transition.

**Create body** — `POST /api/v1/sales-orders`

```json
{
  "number": "SO-0001",
  "customerId": "00000000-0000-0000-0000-000000000000",
  "warehouseId": "00000000-0000-0000-0000-000000000000",
  "currency": "KGS",
  "expectedDateUtc": "2026-07-01T00:00:00Z",
  "notes": "Priority shipment",
  "lines": [
    { "inventoryItemId": "00000000-0000-0000-0000-000000000000", "quantity": 3, "unitPrice": 1500.00 }
  ]
}
```

`POST` returns the new sales order `Guid`. Read models include a computed `totalAmount` (and per-line `lineTotal`).

## Stock Adjustments & Audits Module

Entities described in [[02-Database-Schema]]. Both are document-style operations that follow the same
shape as the stock operations above:

- `GET /...` — list (summary rows)
- `GET /.../{id}` — full document with lines, or `404`
- `POST /...` — create a **Draft**; returns the new `Guid` (`201 Created`)
- `POST /.../{id}/complete` — transition Draft → Completed; `204`, `404`, or `409 Conflict` (not in Draft)
- `POST /.../{id}/cancel` — transition Draft → Cancelled; `204`, `404`, or `409 Conflict`

| Operation   | Base route                   | Complete stock effect                            |
| ----------- | ---------------------------- | ------------------------------------------------ |
| Adjustments | `/api/v1/stock-adjustments`  | applies each line's signed `quantityChange`      |
| Audits      | `/api/v1/stock-audits`       | sets `QuantityOnHand` to each line's `countedQuantity` |

### Create body examples

**Stock adjustment** — `POST /api/v1/stock-adjustments`

`reason` is one of `Correction`, `Damage`, `Loss`, `Theft`, `Found`, `Expired`, `Other`. Line
`quantityChange` is signed (positive adds stock, negative removes it).

```json
{
  "number": "ADJ-0001",
  "warehouseId": "00000000-0000-0000-0000-000000000000",
  "reason": "Damage",
  "notes": "Water damage in aisle 3",
  "lines": [
    { "inventoryItemId": "00000000-0000-0000-0000-000000000000", "quantityChange": -4, "notes": "Crushed boxes" }
  ]
}
```

**Stock audit** — `POST /api/v1/stock-audits`

On creation each line snapshots the item's current on-hand figure as `systemQuantity`. The read model
exposes a per-line `variance` (`countedQuantity − systemQuantity`) and the summary a `totalVariance`.

```json
{
  "number": "AUD-0001",
  "warehouseId": "00000000-0000-0000-0000-000000000000",
  "notes": "Monthly cycle count",
  "lines": [
    { "inventoryItemId": "00000000-0000-0000-0000-000000000000", "countedQuantity": 96 }
  ]
}
```

Completing an audit reconciles stock: each item's `QuantityOnHand` is set to its `countedQuantity`.

## Reporting Module — `/api/v1/reports`

Read-only aggregate reports computed over existing inventory, order, and stock-operation data. The module
adds **no new tables** (see [[02-Database-Schema]]). All endpoints are `GET`, tenant-scoped, and return
`200 OK`.

| Method | Route                               | Description                                              |
| ------ | ----------------------------------- | -------------------------------------------------------- |
| GET    | `/api/v1/reports/inventory-summary` | Inventory KPIs across the catalog                        |
| GET    | `/api/v1/reports/low-stock`         | Active items at or below their reorder level             |
| GET    | `/api/v1/reports/sales-summary`     | Sales order counts + value, broken down by status        |
| GET    | `/api/v1/reports/purchase-summary`  | Purchase order counts + value, broken down by status     |
| GET    | `/api/v1/reports/stock-movements`   | Counts of each stock operation by Draft/Completed/Cancelled |

**`inventory-summary`** response

```json
{
  "totalItems": 120,
  "activeItems": 112,
  "totalQuantityOnHand": 8450.0,
  "itemsBelowReorder": 7,
  "itemsOutOfStock": 2
}
```

**`low-stock`** response — ordered by largest `deficit` (`reorderLevel − quantityOnHand`) first

```json
[
  {
    "id": "00000000-0000-0000-0000-000000000000",
    "sku": "SKU-1001",
    "name": "Cordless Drill",
    "categoryId": "00000000-0000-0000-0000-000000000000",
    "quantityOnHand": 1,
    "reorderLevel": 5,
    "deficit": 4
  }
]
```

**`sales-summary`** / **`purchase-summary`** response — `byStatus` uses the order status names
(`Draft`, `Confirmed`, `Shipped`, `Cancelled` for sales; `Draft`, `Submitted`, `Received`, `Cancelled`
for purchases). Amounts are `Σ(quantity × unitPrice)` over lines.

```json
{
  "totalOrders": 42,
  "totalAmount": 1875000.0,
  "byStatus": [
    { "status": "Draft", "orderCount": 5, "totalAmount": 120000.0 },
    { "status": "Confirmed", "orderCount": 12, "totalAmount": 640000.0 },
    { "status": "Shipped", "orderCount": 23, "totalAmount": 1100000.0 },
    { "status": "Cancelled", "orderCount": 2, "totalAmount": 15000.0 }
  ]
}
```

**`stock-movements`** response — one row per operation type

```json
{
  "operations": [
    { "operation": "Receipts", "draft": 3, "completed": 40, "cancelled": 1, "total": 44 },
    { "operation": "Picks", "draft": 2, "completed": 31, "cancelled": 0, "total": 33 },
    { "operation": "Packs", "draft": 1, "completed": 30, "cancelled": 0, "total": 31 },
    { "operation": "Transfers", "draft": 0, "completed": 9, "cancelled": 1, "total": 10 },
    { "operation": "Adjustments", "draft": 1, "completed": 6, "cancelled": 0, "total": 7 },
    { "operation": "Audits", "draft": 0, "completed": 4, "cancelled": 0, "total": 4 }
  ]
}
```
