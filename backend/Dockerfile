# ─── Stage 1: Build ────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY *.sln ./
COPY global.json ./

# Copy all csproj files in their relative paths
COPY src/WarehouseKG.Api/WarehouseKG.Api.csproj          src/WarehouseKG.Api/
COPY src/WarehouseKG.Application/WarehouseKG.Application.csproj   src/WarehouseKG.Application/
COPY src/WarehouseKG.Domain/WarehouseKG.Domain.csproj          src/WarehouseKG.Domain/
COPY src/WarehouseKG.Infrastructure/WarehouseKG.Infrastructure.csproj src/WarehouseKG.Infrastructure/
COPY tests/WarehouseKG.UnitTests/WarehouseKG.UnitTests.csproj    tests/WarehouseKG.UnitTests/

# Restore dependencies (cached unless csproj changes)
RUN dotnet restore WarehouseKG.sln

# Copy remaining source
COPY src/ ./src/

# Build and publish the API project
RUN dotnet publish src/WarehouseKG.Api/WarehouseKG.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

# ─── Stage 2: Runtime ──────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user and install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/* \
    && useradd appuser

COPY --from=build /app/publish ./

USER appuser

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
    CMD curl --silent --output /dev/null http://127.0.0.1:8080/ || exit 1

ENTRYPOINT ["dotnet", "WarehouseKG.Api.dll"]
