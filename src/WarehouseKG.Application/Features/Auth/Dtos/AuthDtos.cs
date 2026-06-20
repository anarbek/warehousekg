namespace WarehouseKG.Application.Features.Auth.Dtos;

public sealed record AuthResponse(
    Guid UserId,
    string UserName,
    string? Email,
    Guid TenantId,
    IReadOnlyList<string> Roles,
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    Guid? EmployeeId = null);

/// <summary>
/// Outcome of an auth command. <see cref="Succeeded"/> false carries one or more <see cref="Errors"/>;
/// the controller maps these to the appropriate HTTP status.
/// </summary>
public sealed record AuthResult(bool Succeeded, AuthResponse? Response, IReadOnlyList<string> Errors)
{
    public static AuthResult Success(AuthResponse response) => new(true, response, Array.Empty<string>());

    public static AuthResult Fail(params string[] errors) => new(false, null, errors);
}
