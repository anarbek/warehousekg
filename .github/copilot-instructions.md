# WarehouseKG Project Guidelines

Multi-tenant warehouse management system. See `docs/obsidian-vault/00-Project-Overview.md` for full overview.

## Architecture

| Layer | Project | Purpose |
|-------|---------|---------|
| API | `src/WarehouseKG.Api/` | ASP.NET Core controllers, auth, middleware |
| Application | `src/WarehouseKG.Application/` | CQRS commands/queries, DTOs, validators |
| Domain | `src/WarehouseKG.Domain/` | Entities, enums, identity, contracts |
| Infrastructure | `src/WarehouseKG.Infrastructure/` | EF Core, PostgreSQL, identity seeding |

**CQRS with MediatR** — every feature uses `record Command/Query : IRequest<T>` → `class Handler : IRequestHandler<Command, T>`. Controllers inject `ISender` (never `IMediator`). See `src/WarehouseKG.Application/Features/` for examples.

**Multitenancy** — shared DB with `TenantId` discriminator. All entities inherit `BaseEntity`. A global EF Core query filter scopes every query. `TenantId` is set automatically on insert — **never set it manually**. See `docs/obsidian-vault/01-Architecture.md`.

**Auth** — JWT Bearer + hybrid RBAC/PBAC. Every controller action uses `[Authorize(Policy = "{resource}:read|write|delete")]`. Admin role bypasses all checks. See `docs/obsidian-vault/04-Auth-Flow.md`.

**Frontend** — Angular 20 standalone components with DevExtreme 24.2. State via `signal<>()`, DI via `inject()`.
**Mobile** — Flutter with Riverpod, GoRouter, Dio. See `docs/obsidian-vault/08-Mobile-App.md`.

## Key Conventions

### Backend (C# / .NET 10)

- **Records for commands/queries**: `public record CreateXCommand(...) : IRequest<Guid>;` — the record IS the DTO, no separate request class
- **Handlers**: constructor injects `IApplicationDbContext`, method calls `_context.Xxx.Add(entity)` then `SaveChangesAsync`
- **Queries**: always `.AsNoTracking()`, project via `.Select(x => new XxxDto { ... })`
- **Controllers**: `const string R = "resource"` + `[Authorize(Policy = R + ":read")]` on every endpoint
- **CancellationToken**: every handler and controller action takes `CancellationToken cancellationToken`
- **POST returns**: `CreatedAtAction(nameof(GetById), new { id }, id)`
- **PUT/DELETE returns**: `updated ? NoContent() : NotFound()`
- **DELETE policies**: use dedicated `{resource}:delete` — separate from write

### EF Core & Database

- **All entities inherit `BaseEntity`** — provides `Id` (Guid) and `TenantId`
- **TenantId is automatic**: global query filter + `SaveChanges` stamping. Do NOT set it in application code
- **Index pattern**: include `TenantId` as leading column: `(TenantId, Code)`, `(TenantId, Sku)`, etc.
- **Migrations**: `dotnet ef migrations add Name --project src/WarehouseKG.Infrastructure --startup-project src/WarehouseKG.Api`
- **DB update**: `dotnet ef database update --project src/WarehouseKG.Infrastructure --startup-project src/WarehouseKG.Api`

### Angular / Frontend

- **Icons**: DevExtreme built-in only (`trash`, `edit`, `check`, `close`, `add`, `info`, `export`, `play`). Never raw Unicode (✕, ✎). **Invalid icon names** (will render blank): `ruler`, `task`, `checklist`, `box`, `todolist`, `detailslayout`, `movetofolder`, `card`. See `docs/obsidian-vault/12-Design-Guidelines.md` for full reference.
- **Delete buttons**: `icon="trash" type="danger" stylingMode="outlined"`, hidden via `PermissionsService` when user lacks delete permission
- **Workflow buttons**: `icon="check"` for confirm/complete, `icon="close" type="danger"` for cancel
- **Routes**: static routes BEFORE `:id` — otherwise the param catches the static segment
- **Data grids**: `[dataSource]="signal()"` (call the signal), `<dxi-column>` for columns, `<div *dxTemplate>` for custom cells
- **i18n**: `i18n="@@module.page.field"` attributes for Angular extraction

### Flutter / Mobile

- **State**: Riverpod providers (`StateNotifierProvider`, `FutureProvider`)
- **Routing**: GoRouter with auth redirect guard
- **HTTP**: Dio with JWT interceptor + `flutter_secure_storage`
- **Backend URL**: Android emulator uses `http://10.0.2.2:5134` (NOT localhost)
- **Offline-first**: audit module works without internet, syncs when online
- **After JWT claims change** (e.g., new `employee_id` claim): user must logout and re-login to get a fresh token
- **Feature module pattern**: each feature has `models/` (data classes with `fromJson`), `screens/` (UI), `repository.dart` (Riverpod providers + Dio API calls). Register routes in `routes.dart` and endpoints in `api_constants.dart`.

## Build and Test

```powershell
# Backend (port 5134)
dotnet build
dotnet run --project src/WarehouseKG.Api --launch-profile http
dotnet test tests/WarehouseKG.IntegrationTests
dotnet test tests/WarehouseKG.UnitTests

# Frontend (port 4200)
cd frontend; npm start

# Flutter (Android)
cd warehousekg_mobile; flutter run -d emulator-5554

# Database
docker exec -i wkg-postgres psql -U postgres -d WAREHOUSEKG  # prod DB
docker exec wkg-postgres psql -U postgres -c "CREATE DATABASE \"WAREHOUSEKG_TEST\""  # test DB
```

**After any backend change**: create or update integration tests in `tests/WarehouseKG.IntegrationTests/`. Test the happy path AND edge cases (NotFound, InvalidState, auth). Run `dotnet build` first, then `dotnet test`.

**Before running integration tests**: ensure the test database exists:
```powershell
docker exec wkg-postgres psql -U postgres -c "CREATE DATABASE \"WAREHOUSEKG_TEST\""
```
If the DB already exists this is a harmless no-op; if it doesn't, tests will fail with a connection error.

## Bug Patterns to Avoid

- **`SalesOrder.WarehouseId` must be set** — NULL WarehouseId means shipments never appear in inventory movement history
- **Auto-ship inventory deduction** — `CompleteDeliveryStopCommandHandler` must `.Include(so => so.Lines)` and deduct `QuantityOnHand`; don't just set `Status = Shipped`
- **`TenantId` Guid.Empty guard** — `ApplyTenantId()` skips when `tenantId == Guid.Empty`; don't remove that guard
- **EF Core concurrency on update** — update existing child entities in-place by key instead of `RemoveRange` + re-add
- **Angular route order** — static routes before `:id` param routes
- **DevExtreme popup footer** — `dx-toolbar` must be INSIDE `*dxTemplate="let data of 'content'"`, not outside
- **Stale JWT after backend restart** — sessions are invalidated; flush secure storage and re-login on the mobile device

## Integration Tests

- Base class: `IntegrationTestBase` (or `SharedFixture` with `[Collection("IntegrationTests")]`)
- Client: `WarehouseKgClient` wraps `HttpClient` with typed methods
- Payloads: anonymous objects (no DTOs needed in tests)
- Parsing: `System.Text.Json.JsonElement` → `.GetProperty("id").GetString()`
- Test DB: `WAREHOUSEKG_TEST` on same PostgreSQL instance (port 15432)
- Fixtures: `SharedFixture` logs in once as `admin`/`Admin1234!` for all tests

## Documentation

- Full docs: `docs/obsidian-vault/` (Obsidian vault, 12 topic files + session logs)
- Key files: `00-Project-Overview.md`, `01-Architecture.md`, `02-Database-Schema.md`, `03-API-Endpoints.md`, `04-Auth-Flow.md`, `08-Mobile-App.md`, `12-Design-Guidelines.md`
- Dev workflow (restart commands, browser testing): stored in user memory
- Roadmap: `docs/obsidian-vault/09-Roadmap.md`
