# Architecture

System architecture: services, components, data flow, and the decisions behind them.

## Multitenancy

WarehouseKG uses a **shared-database, shared-schema** model with a `TenantId` discriminator on every tenant-owned row (see [[02-Database-Schema]]).

- **Tenant resolution** — an `ITenantProvider` service (defined in the Application layer) resolves the current `TenantId` per request. The `HttpTenantProvider` implementation (Infrastructure layer) reads the `X-Tenant-Id` HTTP header for now; subdomain/host-based resolution is planned later.
- **Isolation** — `WarehouseKgDbContext` applies a **global EF Core query filter** to every entity inheriting `BaseEntity`, so all reads are automatically scoped to the current tenant. The filter references the context's `CurrentTenantId` property, which re-evaluates the provider on each query.
- **Write path** — on `SaveChanges`/`SaveChangesAsync`, newly added `BaseEntity` instances get their `TenantId` stamped from the current tenant automatically.
- **DI wiring** — `AddInfrastructure` registers `IHttpContextAccessor` and `ITenantProvider` (scoped), and the scoped `DbContext` receives the provider via constructor injection.
