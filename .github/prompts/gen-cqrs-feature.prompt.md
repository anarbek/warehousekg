---
description: "Scaffold a complete CQRS feature module: entity, commands, queries, controller, DTOs, and integration tests. Use when: adding a new API resource, creating CRUD endpoints, building a new domain module."
agent: "agent"
argument-hint: "Feature name (e.g., Suppliers, PurchaseOrders)..."
---

# Generate CQRS Feature Module

Scaffold all files needed for a new WarehouseKG feature module, following the exact patterns from the existing codebase.

## What You'll Generate

For a resource like `{Name}` (PascalCase, singular), create these files:

### 1. Domain Entity
`src/WarehouseKG.Domain/Entities/{Name}.cs`
- Inherit `BaseEntity` (provides `Id` + `TenantId`)
- Include: `Code`, `Name`, plus any business-specific properties
- Use `Guid` for foreign keys, `decimal` for quantities/money

### 2. DTOs
`src/WarehouseKG.Application/Features/{Name}s/Dtos/`
- `{Name}Dto.cs` — read model (no TenantId, resolved names from nav properties)
- `{Name}SummaryDto.cs` — list model (fewer fields)

### 3. Commands (CQRS)
`src/WarehouseKG.Application/Features/{Name}s/Commands/`
- `Create{Name}.cs` — `record Create{Name}Command(...) : IRequest<Guid>`
  - Handler: `_context.{Name}s.Add(entity)` → `SaveChangesAsync` → return id
- `Update{Name}.cs` — `record Update{Name}Command(Guid Id, ...) : IRequest<bool>`
  - Handler: find entity, update properties in-place, `SaveChangesAsync`
- `Delete{Name}.cs` — `record Delete{Name}Command(Guid Id) : IRequest<bool>`
  - Handler: find entity, `_context.{Name}s.Remove(entity)`, `SaveChangesAsync`

### 4. Queries (CQRS)
`src/WarehouseKG.Application/Features/{Name}s/Queries/`
- `{Name}Queries.cs` — `Get{Name}sQuery : IRequest<IReadOnlyList<SummaryDto>>` + `Get{Name}ByIdQuery : IRequest<Dto?>`
  - Always `.AsNoTracking()`, project via `.Select(x => new Dto { ... })`

### 5. Controller
`src/WarehouseKG.Api/Controllers/{Name}sController.cs`
- `[Route("api/v1/{name}s")]`, inject `ISender` (not `IMediator`)
- `const string R = "{name}s"` + `[Authorize(Policy = R + ":read|write|delete")]`
- GET → `Ok(result)`, POST → `CreatedAtAction(nameof(GetById), ...)`, PUT/DELETE → `NoContent()/NotFound()`
- Every action takes `CancellationToken cancellationToken`

### 6. Integration Tests
`tests/WarehouseKG.IntegrationTests/{Name}WorkflowTests.cs`
- Use `SharedFixture` with `[Collection("IntegrationTests")]`
- Anonymous object payloads, `JsonElement` parsing
- Test: create → verify in list → workflow (if applicable)
- Helper methods to fetch seed data (first warehouse, first item, etc.)

### 7. Register Policies
- Add `"{name}s"` to `Resources.All` in `src/WarehouseKG.Api/Authorization/Resources.cs`

## Patterns to Follow

- **Records are DTOs**: commands use positional record params — no separate request class needed
- **TenantId is automatic**: never set `entity.TenantId` — the DbContext stamps it on save
- **[Authorize] on every endpoint**: read/write/delete policies
- **Queries use AsNoTracking**: always, no exceptions

Reference the existing `Warehouses` feature for the canonical pattern:
- `src/WarehouseKG.Domain/Entities/Warehouse.cs`
- `src/WarehouseKG.Application/Features/Warehouses/`
- `src/WarehouseKG.Api/Controllers/WarehousesController.cs`
