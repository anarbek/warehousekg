namespace WarehouseKG.Application.Common.Models;

public sealed record IdentityCreationResult(
    bool Succeeded,
    AuthUser? User,
    IReadOnlyList<string> Errors)
{
    public static IdentityCreationResult Success(AuthUser user) => new(true, user, Array.Empty<string>());

    public static IdentityCreationResult Failure(IReadOnlyList<string> errors) => new(false, null, errors);
}
