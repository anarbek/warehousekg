using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Features.Auth.Commands;
using WarehouseKG.Application.Features.Auth.Dtos;

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
    /// Placeholder for Google OAuth sign-in. ClientId/ClientSecret are read from the
    /// <c>Authentication:Google</c> configuration section; the flow is not yet implemented.
    /// </summary>
    [HttpPost("google")]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult Google()
    {
        var clientId = _configuration["Authentication:Google:ClientId"];
        var configured = !string.IsNullOrWhiteSpace(clientId) && clientId != "__PLACEHOLDER__";

        return StatusCode(StatusCodes.Status501NotImplemented, new
        {
            error = "Google OAuth login is not implemented yet.",
            configured
        });
    }
}
