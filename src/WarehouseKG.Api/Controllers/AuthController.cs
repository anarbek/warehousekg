using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WarehouseKG.Api.Authorization;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Auth.Commands;
using WarehouseKG.Application.Features.Auth.Dtos;
using WarehouseKG.Domain.Identity;

namespace WarehouseKG.Api.Controllers;

/// <summary>
/// Authentication endpoints: username/password login, registration, and refresh-token rotation.
/// Google OAuth is stubbed for a future iteration.
/// </summary>
[Route("api/v1/auth")]
[AllowAnonymous]
public class AuthController : ApiControllerBase
{
    private readonly ISender _sender;
    private readonly IConfiguration _configuration;

    public AuthController(ISender sender, IConfiguration configuration)
    {
        _sender = sender;
        _configuration = configuration;
    }

    /// <summary>Registers a new user within a tenant and returns an initial token pair.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.Succeeded
            ? Ok(result.Response)
            : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Authenticates with username + password and returns an access + refresh token pair.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.Succeeded
            ? Ok(result.Response)
            : Unauthorized(new { errors = result.Errors });
    }

    /// <summary>Exchanges a valid refresh token for a new token pair (rotates the refresh token).</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.Succeeded
            ? Ok(result.Response)
            : Unauthorized(new { errors = result.Errors });
    }

    /// <summary>
    /// Returns which resources the current user can read and write, based on roles
    /// and tenant-scoped permission overrides.
    /// </summary>
    [HttpGet("my-permissions")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MyPermissions(
        [FromServices] IApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
        if (string.IsNullOrEmpty(tenantIdClaim)) return Unauthorized();

        var tenantId = Guid.Parse(tenantIdClaim);
        var result = new Dictionary<string, object>();

        foreach (var resource in Resources.All)
        {
            bool canRead = roles.Count > 0;
            bool canWrite = roles.Any(r => r is Roles.Admin or Roles.Manager or Roles.WarehouseOperator);
            bool canDelete = roles.Any(r => r is Roles.Admin or Roles.Manager);

            foreach (var role in roles)
            {
                var perm = await context.TenantPermissions
                    .FirstOrDefaultAsync(p =>
                        p.TenantId == tenantId &&
                        p.RoleName == role &&
                        p.Resource == resource, cancellationToken);

                if (perm != null)
                {
                    canRead = perm.CanRead;
                    canWrite = perm.CanWrite;
                    canDelete = perm.CanDelete;
                    break;
                }
            }

            if (roles.Contains(Roles.Admin))
            {
                canRead = true;
                canWrite = true;
                canDelete = true;
            }

            result[resource] = new { canRead, canWrite, canDelete };
        }

        return Ok(new { resources = result, roles });
    }

    /// <summary>
    /// Initiates Google OAuth sign-in by challenging the Google authentication scheme.
    /// The browser is redirected to Google's consent screen; after the user approves,
    /// Google redirects back to <c>/api/v1/auth/google-callback</c> (handled by the
    /// middleware) which then redirects to <c>/api/v1/auth/google-complete</c>.
    /// </summary>
    [HttpGet("google")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult Google()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleComplete))
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Called by the Google OAuth middleware after a successful authorization code exchange.
    /// Finds or creates a local user from the Google identity, issues a JWT pair, and
    /// redirects the browser to the Angular frontend with the tokens in the query string.
    /// </summary>
    [HttpGet("google-complete")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleComplete(CancellationToken cancellationToken)
    {
        // Authenticate against the short-lived OAuth cookie set by the Google middleware.
        var authResult = await HttpContext.AuthenticateAsync("GoogleOAuth");
        if (!authResult.Succeeded || authResult.Principal is null)
        {
            return Unauthorized(new { error = "Google authentication failed." });
        }

        var principal = authResult.Principal;
        var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = principal.FindFirstValue(ClaimTypes.Email);
        var name = principal.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrWhiteSpace(sub))
        {
            return Unauthorized(new { error = "Google did not return a subject claim." });
        }

        // Parse the tenant to use; fall back to the configured default or Guid.Empty.
        var tenantIdStr = _configuration["Authentication:Google:DefaultTenantId"];
        var tenantId = Guid.TryParse(tenantIdStr, out var tid) ? tid : Guid.Empty;

        var command = new LoginWithGoogleCommand(sub, email, name, tenantId);
        var result = await _sender.Send(command, cancellationToken);

        // Clear the temporary OAuth cookie – it is no longer needed.
        await HttpContext.SignOutAsync("GoogleOAuth");

        if (!result.Succeeded || result.Response is null)
        {
            return Unauthorized(new { errors = result.Errors });
        }

        var r = result.Response;
        var frontendBase = _configuration["Frontend:BaseUrl"] ?? "http://localhost:4200";

        var qs = System.Web.HttpUtility.ParseQueryString(string.Empty);
        qs["accessToken"] = r.AccessToken;
        qs["refreshToken"] = r.RefreshToken;
        qs["userId"] = r.UserId.ToString();
        qs["userName"] = r.UserName;
        qs["email"] = r.Email ?? string.Empty;
        qs["tenantId"] = r.TenantId.ToString();
        qs["roles"] = string.Join(",", r.Roles);
        qs["expiresAt"] = r.AccessTokenExpiresAtUtc.ToString("O");

        return Redirect($"{frontendBase}/auth/callback?{qs}");
    }
}
