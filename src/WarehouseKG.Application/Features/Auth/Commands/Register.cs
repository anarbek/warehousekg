using MediatR;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Auth.Dtos;
using WarehouseKG.Domain.Identity;

namespace WarehouseKG.Application.Features.Auth.Commands;

public record RegisterCommand(
    string UserName,
    string? Email,
    string Password,
    Guid TenantId,
    string? Role) : IRequest<AuthResult>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IRefreshTokenStore _refreshTokenStore;

    public RegisterCommandHandler(
        IIdentityService identityService,
        IJwtTokenGenerator tokenGenerator,
        IRefreshTokenStore refreshTokenStore)
    {
        _identityService = identityService;
        _tokenGenerator = tokenGenerator;
        _refreshTokenStore = refreshTokenStore;
    }

    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var role = string.IsNullOrWhiteSpace(request.Role) ? Roles.Viewer : request.Role;
        if (!Roles.IsValid(role))
        {
            return AuthResult.Fail($"'{role}' is not a valid role.");
        }

        var creation = await _identityService.CreateUserAsync(
            request.UserName,
            request.Email,
            request.Password,
            request.TenantId,
            role,
            cancellationToken);

        if (!creation.Succeeded || creation.User is null)
        {
            return AuthResult.Fail(creation.Errors.ToArray());
        }

        var response = await AuthTokenIssuer.IssueAsync(
            creation.User,
            _tokenGenerator,
            _refreshTokenStore,
            cancellationToken);

        return AuthResult.Success(response);
    }
}
