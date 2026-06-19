using System.Reflection;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WarehouseKG.Api.Authorization;
using WarehouseKG.Application;
using WarehouseKG.Infrastructure;
using WarehouseKG.Infrastructure.Identity;
using WarehouseKG.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Scheme name for the short-lived cookie used during the Google OAuth dance only.
const string GoogleOAuthScheme = "GoogleOAuth";

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ─── OpenTelemetry ────────────────────────────────────────────────────────
var otelEndpoint = builder.Configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317";
var serviceName = "WarehouseKG.Api";
var serviceVersion = "1.0.0";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName, serviceVersion))
    .WithTracing(t =>
    {
        t.AddAspNetCoreInstrumentation();
        t.AddHttpClientInstrumentation();
        t.AddEntityFrameworkCoreInstrumentation();
        t.AddOtlpExporter(o => o.Endpoint = new Uri(otelEndpoint));
    })
    .WithMetrics(m =>
    {
        m.AddAspNetCoreInstrumentation();
        m.AddHttpClientInstrumentation();
        m.AddOtlpExporter(o => o.Endpoint = new Uri(otelEndpoint));
    });

builder.Logging.AddOpenTelemetry(o =>
{
    o.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion));
    o.IncludeFormattedMessage = true;
    o.IncludeScopes = true;
    o.AddOtlpExporter(ex => ex.Endpoint = new Uri(otelEndpoint));
});
// ─── End OpenTelemetry ────────────────────────────────────────────────────

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var signingKey = jwtSection["SigningKey"] ?? string.Empty;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "unique_name",
            RoleClaimType = ClaimTypes.Role
        };
    })
    // Temporary cookie used only to carry the Google identity during the OAuth dance.
    // It is consumed and cleared in the google-complete endpoint.
    .AddCookie(GoogleOAuthScheme, options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? string.Empty;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? string.Empty;
        options.SignInScheme = GoogleOAuthScheme;
        // The middleware handles this path; it is NOT a controller action.
        options.CallbackPath = "/api/v1/auth/google-callback";
        options.Scope.Add("email");
        options.Scope.Add("profile");
    });

builder.Services.AddWarehouseAuthorization();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WarehouseKG API",
        Version = "v1",
        Description = "Warehouse management system API (inventory module)."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter the JWT access token (without the 'Bearer ' prefix).",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

await SeedRolesAsync(app);
await SeedAdminUserAsync(app);
await SeedDefaultPermissionsAsync(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "WarehouseKG API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Ensures the canonical roles exist. Wrapped so a transient DB outage doesn't block startup in dev.
static async Task SeedRolesAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    try
    {
        await IdentitySeeder.SeedRolesAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        logger.LogWarning(ex, "Role seeding skipped (database may be unavailable).");
    }
}

// Ensures the first registered user has Admin role for development convenience.
static async Task SeedAdminUserAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    try
    {
        await IdentitySeeder.SeedAdminUserAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        logger.LogWarning(ex, "Admin user seeding skipped.");
    }
}

// Seeds default TenantPermission rows for new roles (Auditor, Dispatcher, HR).
static async Task SeedDefaultPermissionsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    try
    {
        await IdentitySeeder.SeedDefaultPermissionsAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        logger.LogWarning(ex, "Default permission seeding skipped.");
    }
}
