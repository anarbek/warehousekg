# 13 — Copilot Customizations

VS Code Copilot customization files that encode project conventions, automate scaffolding, and restrict tool access for different workflows.

## File Inventory

```
.github/
├── copilot-instructions.md              # Always-on project conventions (loaded into every chat)
├── prompts/
│   ├── gen-cqrs-feature.prompt.md       # /gen-cqrs-feature — scaffold backend feature module
│   └── gen-flutter-screen.prompt.md     # /gen-flutter-screen — scaffold Flutter feature module
└── agents/
    └── warehousekg-tester.agent.md      # Test-only agent (read/search/execute, no edit)
```

---

## Customization Types

| Type | File Pattern | Purpose | Invocation |
|------|-------------|---------|------------|
| **Instructions** | `copilot-instructions.md` | Always-on project rules — CQRS patterns, auth, icons, test conventions | Automatic (every chat) |
| **Prompts** | `*.prompt.md` | Single-task templates for scaffolding | Type `/` in chat → select |
| **Agents** | `*.agent.md` | Custom personas with restricted tools | Agent picker or as subagent |

See the [agent-customization skill](https://code.visualstudio.com/docs/copilot/customization/overview) for all available primitives (skills, hooks, file instructions).

---

## `.github/copilot-instructions.md`

Loaded automatically into every chat session. Encodes:

- **Architecture**: CQRS layers, multitenancy with `BaseEntity`, JWT + RBAC/PBAC
- **Backend conventions**: record-as-DTO pattern, `ISender` (not `IMediator`), `.AsNoTracking()` for queries
- **EF Core rules**: `TenantId` is automatic — never set manually, index pattern `(TenantId, Code)`
- **Angular rules**: DevExtreme icons only (no raw Unicode), `signal<>()` pattern, route ordering
- **Flutter rules**: Riverpod + GoRouter + Dio, `10.0.2.2:5134` for emulator, offline-first
- **Build commands**: `dotnet build`, `npm start`, `flutter run`, DB commands
- **Bug patterns**: 7 known gotchas to avoid (WarehouseId NULL, missing `.Include`, etc.)
- **Integration test conventions**: `SharedFixture`, `WarehouseKgClient`, anonymous payloads

When to edit: whenever a new convention is established or a bug pattern is discovered.

---

## `/gen-cqrs-feature`

Scaffolds a complete backend feature module. Generates 7 files:

1. Domain entity (`Domain/Entities/{Name}.cs`)
2. DTOs (`Application/Features/{Name}s/Dtos/`)
3. Commands: Create, Update, Delete
4. Queries: GetList, GetById
5. Controller (`Api/Controllers/{Name}sController.cs`)
6. Integration tests
7. Policy registration in `Resources.cs`

All files follow the exact CQRS + MediatR patterns from existing features like `Warehouses`.

### Example

Type `/gen-cqrs-feature` in chat → enter `Suppliers` → agent generates entity, commands, queries, controller, DTOs, tests, and policy entry.

---

## `/gen-flutter-screen`

Scaffolds a new Flutter feature module. Generates:

1. Models (`features/{name}/models/{name}_models.dart`)
2. Repository with Riverpod providers and Dio API calls
3. Screens: list view + detail view (+ workflow screen if applicable)
4. GoRouter route entries
5. API endpoint constants
6. Dashboard tile (if needed)

All files follow the patterns from existing features: `audit` (offline-first) and `dispatching` (workflow with status transitions).

### Example

Type `/gen-flutter-screen` in chat → enter `transfers` → agent generates models, screens, repository, routes, and API constants.

---

## `warehousekg-tester` Agent

A custom agent persona specialized in integration testing. **Cannot edit source code** — only `read`, `search`, and `execute` (for `dotnet build`/`dotnet test`).

### Use cases

| Scenario | How to use |
|----------|-----------|
| Write tests for a new endpoint | Select `warehousekg-tester` from agent picker → describe the endpoint |
| Run existing tests | Ask: "Run all integration tests" |
| Debug failing tests | Ask: "Run XxxWorkflowTests and report failures" |
| Test from another agent | The parent agent can invoke it as a subagent: "Delegate test writing to warehousekg-tester" |

### Tool restrictions

| Tool | Allowed? |
|------|----------|
| `read` (read files) | ✅ |
| `search` (search codebase) | ✅ |
| `execute` (run commands) | ✅ |
| `edit` (edit files) | ❌ |
| `web` (fetch URLs) | ❌ |
| `agent` (invoke subagents) | ❌ |

This prevents accidental edits to production code while testing.

---

## When to Create Each Type

| Need | Use |
|------|-----|
| Convention that applies to ALL work | Instructions (`copilot-instructions.md`) |
| Repeated scaffolding task | Prompt (`*.prompt.md`) |
| Role with tool restrictions | Agent (`*.agent.md`) |
| Multi-step workflow with bundled assets | Skill (`SKILL.md` + `scripts/`) |
| Deterministic enforcement | Hook (`*.json` + shell script) |

---

## See Also

- [[00-Project-Overview]] — project structure and modules
- [[01-Architecture]] — backend design decisions
- [[12-Design-Guidelines]] — DevExtreme conventions encoded in instructions
- [[09-Roadmap]] — planned features that will use these prompts
