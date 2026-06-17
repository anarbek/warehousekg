# Roadmap

Planned features, milestones, and the prioritized backlog over time.

## Open items

- **Per-location stock levels** — introduce a `StockLevel (InventoryItem × WarehouseLocation → Quantity)`
  entity so receiving, picking, transfers, adjustments, and audits adjust stock per warehouse/location
  instead of a single tenant-wide `InventoryItem.QuantityOnHand`. See [[02-Database-Schema]].
- **Validation** — add request validation (e.g. FluentValidation) for commands, including checks that
  referenced warehouses/items/locations exist and that quantities are positive.
- **Stock guards** — prevent picks/transfers that would drive stock negative.
- **Reporting performance** — the `/api/v1/reports` endpoints aggregate over live tables on each request.
  As volumes grow, back them with database views, pre-aggregated summary tables, or date-range filters and
  pagination. See [[03-API-Endpoints]].
- **Authorization rollout** — ~~apply the role policies (`RequireAdmin` … `RequireViewer`) to the domain
  controllers; today only the auth endpoints exist and other controllers are still open.~~ ✅ Done. See [[04-Auth-Flow]].
- **Google OAuth** — implement `POST /api/v1/auth/google` (currently a `501` stub): validate the Google ID
  token, provision/match an `ApplicationUser` per tenant, then issue the standard JWT + refresh pair. Config
  placeholders already exist under `Authentication:Google`. See [[04-Auth-Flow]].
