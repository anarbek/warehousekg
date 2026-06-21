# Project Overview

WarehouseKG is a multi-tenant warehouse management system with:

- **Backend**: .NET 10 ASP.NET Core, CQRS with MediatR, EF Core + PostgreSQL
- **Frontend**: Angular 20, DevExtreme 24.2, esbuild dev server
- **Mobile**: Flutter 3.10.3, Riverpod, GoRouter, Dio — offline-first inventory auditing
- **Auth**: JWT Bearer, tenant-based multi-tenancy with resource-level RBAC + PBAC

## Repository Structure

```
warehousekg/
├── .github/                        # Copilot customizations (instructions, prompts, agents)
│   ├── copilot-instructions.md
│   ├── prompts/
│   └── agents/
├── src/
│   ├── WarehouseKG.Api/            # ASP.NET Core controllers, auth, middleware
│   ├── WarehouseKG.Application/    # CQRS commands, queries, DTOs, validators
│   ├── WarehouseKG.Domain/         # Entities, enums, identity, common contracts
│   └── WarehouseKG.Infrastructure/ # EF Core, PostgreSQL, identity seeding
├── frontend/                       # Angular 20 SPA
├── warehousekg_mobile/             # Flutter mobile app (Android)
├── tests/
│   ├── WarehouseKG.UnitTests/
│   └── WarehouseKG.IntegrationTests/
├── docs/obsidian-vault/            # Project documentation
└── docker/                         # Docker Compose for observability stack
```

## Key Modules

| Module | Backend | Frontend | Mobile |
|---|---|---|---|
| Inventory | ✅ CRUD | ✅ Full | - |
| Stock Receipts | ✅ | ✅ Full | - |
| Stock Adjustments | ✅ | ✅ Full | - |
| Stock Audits | ✅ | ✅ Full (detail, delete) | ✅ v1 (offline count + sync) |
| Stock Transfers | ✅ | ✅ Full | - |
| Pick Orders | ✅ | ✅ Full | - |
| Purchase Orders | ✅ | ✅ Full | - |
| Sales Orders | ✅ | ✅ Full | - |
| Personnel | ✅ | ✅ Full | - |
| Vehicle Fleet | ✅ | ✅ Full | - |
| Reporting | ✅ | ✅ Dashboard | - |
| Admin | ✅ | ✅ Users, permissions | - |
| Superadmin | ✅ | ✅ Tenant CRUD + seed | - |

## Ports

| Service | Port | Notes |
|---|---|---|
| Backend API | 5134 | `http://localhost:5134` |
| Frontend | 4200 | `http://localhost:4200`, esbuild |
| PostgreSQL | 15432 | Docker `wkg-postgres`, user `postgres`, db `WAREHOUSEKG` |
| Flutter | emulator-5554 | Android emulator, connects via `10.0.2.2:5134` |

## See Also

- [[01-Architecture]] — Backend design, CQRS, MediatR pipeline
- [[02-Database-Schema]] — Tables, relationships, Entity Framework config
- [[03-API-Endpoints]] — Full REST API reference
- [[04-Auth-Flow]] — JWT, tenant multi-tenancy, RBAC permissions
- [[08-Mobile-App]] — Flutter app architecture and features
- [[09-Roadmap]] — Completed and planned features
- [[13-Copilot-Customizations]] — VS Code Copilot instructions, prompts, and agents
