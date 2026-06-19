using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Infrastructure.Identity;

namespace WarehouseKG.Infrastructure.Persistence;

// Used by the EF Core tools (e.g. "dotnet ef migrations add" / "database update") so the
// design-time context does not depend on the running web host or an HTTP request for the
// tenant. The connection string is read from the startup project's appsettings + environment
// variables, so it stays in sync with what the app uses at runtime.
public class WarehouseKgDbContextFactory : IDesignTimeDbContextFactory<WarehouseKgDbContext>
{
    public WarehouseKgDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            // Matches <UserSecretsId> in WarehouseKG.Api.csproj so migrations read the same
            // secret the running app uses. User secrets override appsettings; env vars win last.
            .AddUserSecrets("warehousekg-api")
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'Default' was not found. Ensure appsettings.Development.json in the " +
                "startup project (WarehouseKG.Api) contains ConnectionStrings:Default, or set the " +
                "ConnectionStrings__Default environment variable.");
        }

        var options = new DbContextOptionsBuilder<WarehouseKgDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new WarehouseKgDbContext(options, new DesignTimeTenantProvider(), new DesignTimeCurrentUser());
    }

    private sealed class DesignTimeTenantProvider : ITenantProvider
    {
        public Guid GetTenantId() => Guid.Empty;
    }

    private sealed class DesignTimeCurrentUser : ICurrentUserService
    {
        public string? UserName => "migrations";
        public Guid? UserId => null;
    }
}
