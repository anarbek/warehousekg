# Warehouse Management System (WarehouseKG) — Cursor Build Prompt

## Project Overview
Build a multi-tenant warehouse management system (WMS) for clients in Kyrgyzstan. The system must support Russian (default) and Kyrgyz languages across all user-facing components.

---

## 1. Architecture & Tech Stack

- **Backend**: .NET 10 (ASP.NET Core Web API), Clean Architecture (Domain, Application, Infrastructure, API layers)
- **Frontend (Web)**: Angular (latest), standalone components, Angular Material or PrimeNG, ngx-translate or @angular/localize for i18n (ru/ky)
- **Mobile**: Flutter/Dart app (Android + iOS), shared API consumption, same auth providers
- **Database**: PostgreSQL 16+
- **Containerization**: Docker + Docker Compose (separate compose files for dev/prod), multi-stage Dockerfiles for backend, frontend, mobile build artifacts
- **Observability**: OpenTelemetry (traces, metrics, logs) → OTEL Collector → Prometheus/Loki → Grafana dashboards
- **Multitenancy**: Shared database with TenantId discriminator (or schema-per-tenant — recommend discriminator for simplicity), tenant resolution via subdomain or header
- **Authentication**: OAuth2/OpenID Connect via ASP.NET Identity + external providers (max 4): Google, Facebook, Microsoft, Apple (Apple recommended for iOS App Store compliance). JWT-based session tokens.

---

## 2. Database Configuration

Server=localhost;Port=5432;Database=WAREHOUSEKG;User ID=postgres;Password=P@@ssw0rd!+;Enlist=true;Maximum Pool Size=500;Minimum Pool Size=20;Connection Idle Lifetime=300;Connection Lifetime=600;Timeout=30;Command Timeout=60;Pooling=true;

- Use EF Core with Npgsql provider
- Apply migrations per-tenant aware (global query filters on TenantId)
- Store connection string in `appsettings.json` for dev, environment variables / secrets manager for prod containers

---

## 3. Core Modules (suggested for warehouse domain)

- **Tenants & Organizations**: tenant onboarding, settings, branding
- **Users & Roles**: RBAC (Admin, Manager, Warehouse Operator, Viewer)
- **Inventory Management**: items, SKUs, categories, units of measure, barcode/QR support
- **Warehouses & Locations**: multiple warehouses, zones, bins/shelves
- **Stock Operations**: receiving, picking, packing, shipping, transfers between warehouses
- **Stock Adjustments & Audits**: cycle counts, write-offs
- **Purchase Orders & Suppliers**
- **Sales Orders & Customers**
- **Reporting & Analytics**: stock levels, movement history, low-stock alerts
- **Notifications**: email/push for low stock, order status changes
- **Audit Log**: track all critical changes (who/when/what)
- **Localization**: ru-RU (default), ky-KG — all UI strings, date/number/currency formats (KGS som)

---

## 4. Authentication & Authorization

- ASP.NET Core Identity + external OAuth providers (configure max 4):
  1. Google
  2. Facebook
  3. Microsoft
  4. Apple (or VKontakte/Yandex as alternatives popular in CIS region — confirm with client)
- JWT access + refresh tokens
- Multi-tenant aware: users belong to one or more tenants, tenant context passed via claims
- 2FA optional (recommended for Admin roles)

---

## 5. Observability (OpenTelemetry + Grafana)

- Instrument .NET API with `OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Instrumentation.AspNetCore`, `OpenTelemetry.Instrumentation.Http`, `OpenTelemetry.Instrumentation.EntityFrameworkCore`
- Export traces/metrics via OTLP to an OpenTelemetry Collector container
- Collector forwards: metrics → Prometheus, traces → Tempo/Jaeger, logs → Loki
- Grafana container with pre-provisioned dashboards (API latency, error rates, DB query performance, request volume per tenant)
- Add `docker-compose.observability.yml` with otel-collector, prometheus, grafana, loki, tempo services

---

## 6. Containerization

- `Dockerfile` for backend (multi-stage: build + runtime, .NET 10 SDK/ASP.NET base images)
- `Dockerfile` for Angular frontend (multi-stage: node build + nginx serve)
- `docker-compose.yml`: postgres, backend API, frontend, otel-collector, prometheus, grafana, loki
- `.env` file for secrets/config, never committed
- Health checks for all services
- Nginx reverse proxy config with tenant subdomain routing

---

## 7. Mobile App (Dart/Flutter)

- Flutter app targeting Android & iOS
- Shared design tokens/theme matching web branding, ru/ky localization via `flutter_localizations` + `intl`
- Auth: same OAuth providers (google_sign_in, flutter_facebook_auth, sign_in_with_apple)
- Offline-first considerations: local caching (Hive/SQLite) for warehouse scanning workflows
- Barcode/QR scanning via camera (mobile_scanner package)
- Push notifications (Firebase Cloud Messaging)

---

## 8. Documentation Folder (Obsidian Vault)

Create folder `/docs/obsidian-vault/` containing markdown notes:
- `00-Project-Overview.md` — goals, stakeholders, scope
- `01-Architecture.md` — diagrams, tech stack decisions
- `02-Database-Schema.md` — ERD, table definitions
- `03-API-Endpoints.md` — endpoint list, request/response examples
- `04-Auth-Flow.md` — OAuth provider setup, token flow diagrams
- `05-Localization.md` — translation key conventions, ru/ky glossary
- `06-Deployment.md` — Docker/compose instructions, environment variables
- `07-Observability.md` — Grafana dashboard guide, OTEL setup
- `08-Mobile-App.md` — Flutter setup, build/release process
- `09-Roadmap.md` — future features, open questions
- Use Obsidian internal links (`[[note-name]]`) to cross-reference notes

---

## 9. Additional Recommended Features

- **Audit trail & change history** for compliance
- **Role-based dashboards** (different views for managers vs operators)
- **Barcode/QR label generation & printing**
- **CSV/Excel import-export** for bulk inventory updates
- **Multi-currency support** (KGS primary, USD/RUB secondary for cross-border suppliers)
- **API rate limiting & throttling** per tenant
- **Automated backups** for PostgreSQL
- **CI/CD pipeline** (GitHub Actions) for build/test/deploy
- **API documentation** via Swagger/OpenAPI with versioning
- **Data export for tax/accounting compliance** (1C integration is common in CIS region — consider as future module)
- **SMS notifications** (popular in Kyrgyzstan via local providers like Beeline/MegaCom/O!)
- **Dark mode** for web/mobile
- **Tenant-level feature flags**

---

## 10. Deliverables Checklist

- [ ] Solution structure (.NET 10 Clean Architecture)
- [ ] Angular workspace with i18n configured (ru default, ky secondary)
- [ ] Flutter project with localization and auth
- [ ] Docker Compose stack (app + db + observability)
- [ ] OpenTelemetry instrumentation wired to Grafana
- [ ] Auth with up to 4 OAuth providers
- [ ] Obsidian vault with initial documentation structure
- [ ] Seed data/migration scripts for PostgreSQL (WAREHOUSEKG)