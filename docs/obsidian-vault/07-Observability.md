# Observability

Logging, metrics, tracing, and alerting strategy for monitoring the system.

## Overview

WarehouseKG.Api is instrumented with **OpenTelemetry**, exporting traces, metrics, and logs
via OTLP to an OpenTelemetry Collector. The collector forwards:

| Signal   | Backend      | Port  |
| -------- | ------------ | ----- |
| Traces   | **Tempo**    | 3200  |
| Metrics  | **Prometheus** | 9090 |
| Logs     | **Loki**     | 3100  |

**Grafana** (port 3000) is pre-configured with all three datasources and trace-to-log linking.

## Instrumentation

The .NET API instruments:

- **ASP.NET Core** — inbound HTTP request traces, duration, status codes
- **HttpClient** — outbound HTTP call traces
- **EF Core** — database query traces (executed SQL, duration)
- **Logs** — structured log export via `ILogger` → OTLP

Configuration in `appsettings.json`:

```json
{
  "OpenTelemetry": {
    "Endpoint": "http://localhost:4317"
  }
}
```

## Quick Start

```bash
# From the repository root
cd docker

# Start only the observability stack
docker compose -f docker-compose.observability.yml up -d

# Verify all services
docker compose -f docker-compose.observability.yml ps
```

## Accessing Grafana

1. Open **http://localhost:3000**
2. Login: `admin` / `admin`
3. **Explore** tab — select Prometheus, Tempo, or Loki

### Pre-configured datasources

| Name       | Type       | URL                    |
| ---------- | ---------- | ---------------------- |
| Prometheus | prometheus | http://prometheus:9090 |
| Tempo      | tempo      | http://tempo:3200      |
| Loki       | loki       | http://loki:3100       |

### Trace → Log correlation

Traces in Tempo link to logs in Loki via `trace_id` — click a span, then
"Related logs" to see the corresponding log entries.

## Stopping

```bash
docker compose -f docker-compose.observability.yml down -v
```

The `-v` flag removes persisted data volumes (metrics, traces, logs).

## Port Map

| Service         | Port | Purpose              |
| --------------- | ---- | -------------------- |
| OTEL Collector  | 4319 | OTLP gRPC receiver (mapped from 4317) |
| OTEL Collector  | 4320 | OTLP HTTP receiver (mapped from 4318) |
| Prometheus      | 9091 | Metrics query (mapped from container 9090) |
| Tempo           | 3201 | Trace query (mapped from container 3200) |
| Loki            | 3101 | Log query (mapped from container 3100) |
| Grafana         | 3000 | Dashboards           |

## Files

```
docker/
├── docker-compose.observability.yml
├── otel-collector-config.yaml
├── prometheus.yml
└── grafana/
    └── datasources/
        └── datasources.yaml
```
