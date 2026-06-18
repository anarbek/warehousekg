# Deployment

Build, release, and deployment process: environments, Docker, and CI/CD pipelines.

---

## Prerequisites

- Docker Engine 24+ with Docker Compose v2.20+ (required for the `include` directive used to merge the observability stack)
- Git (to clone the repository)

---

## Quick Start (Local Development)

### 1. Clone and navigate

```bash
git clone <repo-url> warehousekg
cd warehousekg
```

### 2. Create your `.env` file

```bash
cp .env.example .env
```

Edit `.env` and fill in the mandatory secrets:

| Variable | Required | Description |
|---|---|---|
| `DB_PASSWORD` | **Yes** | PostgreSQL password for the `postgres` user |
| `JWT_SIGNING_KEY` | **Yes** | At least 32 random characters for JWT token signing |
| `GOOGLE_CLIENT_ID` | Optional | Google OAuth client ID |
| `GOOGLE_CLIENT_SECRET` | Optional | Google OAuth client secret |
| `FACEBOOK_CLIENT_ID` | Optional | Facebook App ID |
| `FACEBOOK_CLIENT_SECRET` | Optional | Facebook App secret |
| `MICROSOFT_CLIENT_ID` | Optional | Microsoft Entra ID application ID |
| `MICROSOFT_CLIENT_SECRET` | Optional | Microsoft Entra ID client secret |
| `APPLE_CLIENT_ID` | Optional | Apple Services ID |
| `APPLE_CLIENT_SECRET` | Optional | Apple client secret |

All other variables have sensible defaults (see `.env.example`).

### 3. Start the stack

```bash
docker compose up -d
```

The first build will take a few minutes (downloading images, restoring NuGet/npm packages, compiling). Subsequent starts are near-instant.

### 4. Verify

```bash
docker compose ps
```

All services should show `healthy` after ~30 seconds.

---

## Services & Ports

| Service | Container | Host Port | Internal Port | Health Check |
|---|---|---|---|---|
| **Frontend** (Angular → nginx) | `wkg-frontend` | `4200` | `80` | `wget /` |
| **Backend API** (.NET 10) | `wkg-backend` | `5134` | `8080` | `curl /` |
| **PostgreSQL** 16 | `wkg-postgres` | `5432` | `5432` | `pg_isready` |
| **Grafana** | `wkg-grafana` | `3000` | `3000` | built-in |
| **Prometheus** | `wkg-prometheus` | `9091` | `9090` | built-in |
| **Tempo** (traces) | `wkg-tempo` | `3201` | `3200` | built-in |
| **Loki** (logs) | `wkg-loki` | `3101` | `3100` | built-in |
| **OTEL Collector** | `wkg-otel-collector` | `4319` (gRPC), `4320` (HTTP) | `4317`, `4318` | — |

### Access points

| URL | Purpose |
|---|---|
| [http://localhost:4200](http://localhost:4200) | WarehouseKG web application (Russian) |
| [http://localhost:4200/ky/](http://localhost:4200/ky/) | WarehouseKG web application (Kyrgyz) |
| [http://localhost:3000](http://localhost:3000) | Grafana dashboards (admin / admin) |
| [http://localhost:5134/swagger](http://localhost:5134/swagger) | Swagger API docs (if enabled) |

---

## Docker Architecture

```
┌──────────────────────────────────────────────────┐
│                  localhost                        │
│                                                   │
│  :4200           :5134           :5432            │
│    │               │               │              │
│  ┌─▼──────────┐  ┌─▼──────────┐  ┌─▼──────────┐  │
│  │  frontend  │  │  backend   │  │  postgres  │  │
│  │  nginx:80  │──▶│  .NET:8080 │──▶│    :5432   │  │
│  └────────────┘  └─────┬──────┘  └────────────┘  │
│                        │                          │
│                   OTLP │ gRPC :4317               │
│                        ▼                          │
│  ┌──────────────────────────────────────────┐    │
│  │        Observability Stack                │    │
│  │  otel-collector → tempo / prom / loki     │    │
│  │                    └── grafana :3000 ──── │    │
│  └──────────────────────────────────────────┘    │
└──────────────────────────────────────────────────┘
```

The frontend nginx proxies `/api/*` requests to `backend:8080` inside the Docker network, forwarding tenant headers.

---

## Common Commands

```bash
# View logs for a specific service
docker compose logs -f backend

# Restart a single service
docker compose restart backend

# Rebuild after code changes
docker compose up -d --build

# Stop everything
docker compose down

# Stop and remove volumes (⚠ destroys DB data)
docker compose down -v

# Check health status
docker compose ps
```

---

## Environment Variables Reference

The backend reads configuration from these environment variables (`.NET` convention: `Section__Key` maps to `Section:Key` in `appsettings.json`):

| Variable | Maps to | Default |
|---|---|---|
| `ConnectionStrings__Default` | Full Npgsql connection string | (built from DB_* vars) |
| `DB_HOST` | PostgreSQL hostname | `postgres` |
| `DB_PORT` | PostgreSQL port | `5432` |
| `DB_NAME` | Database name | `WAREHOUSEKG` |
| `DB_USER` | Database user | `postgres` |
| `DB_PASSWORD` | Database password | **required** |
| `Jwt__SigningKey` | HMAC signing key for JWT | **required** |
| `Jwt__Issuer` | JWT issuer claim | `WarehouseKG` |
| `Jwt__Audience` | JWT audience claim | `WarehouseKG` |
| `Jwt__AccessTokenMinutes` | Access token TTL (minutes) | `15` |
| `Jwt__RefreshTokenDays` | Refresh token TTL (days) | `7` |
| `OpenTelemetry__Endpoint` | OTEL Collector gRPC endpoint | `http://otel-collector:4317` |
| `Authentication__Google__ClientId` | Google OAuth client ID | — |
| `Authentication__Google__ClientSecret` | Google OAuth client secret | — |
| … (Facebook, Microsoft, Apple) | Same pattern | — |

---

## Production Considerations

- **HTTPS**: Place an HTTPS-terminating reverse proxy (nginx, Traefik, Caddy) in front of the stack. The internal Docker network uses plain HTTP.
- **Secrets**: Use Docker secrets or a vault (HashiCorp Vault, Azure Key Vault) instead of `.env` files.
- **DB Backups**: Schedule `pg_dump` or use `pgBackRest`. Mount a backup volume.
- **Resource limits**: Add `deploy.resources.limits` sections in `docker-compose.yml` for CPU/memory.
- **CI/CD**: See [[09-Roadmap]] for planned GitHub Actions pipelines.

---

## Troubleshooting

| Symptom | Likely Cause | Fix |
|---|---|---|
| `backend` fails to start | DB not ready | Wait for `postgres healthy`, then `docker compose restart backend` |
| `DB_PASSWORD must be set` | Missing `.env` file | `cp .env.example .env` and edit |
| `frontend` returns 502 | Backend not healthy | Check `docker compose logs backend` |
| Auth redirects fail | `Frontend:BaseUrl` mismatch | Set env var in compose to match your public URL |
| Grafana login fails | Wrong credentials | Default is `admin` / `admin` (change in `.env`) |
