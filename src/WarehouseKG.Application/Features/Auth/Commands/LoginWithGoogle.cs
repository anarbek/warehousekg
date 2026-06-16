using MediatR;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Auth.Dtos;

namespace WarehouseKG.Application.Features.Auth.Commands;

/// <summary>
/// Finds or creates a local user from a verified Google identity, then issues a token pair.
/// </summary>
public record LoginWithGoogleCommand(
    string Sub,
    string? Email,
    string? DisplayName,
    Guid TenantId) : IRequest<AuthResult>;

public class LoginWithGoogleCommandHandler : IRequestHandler<LoginWithGoogleCommand, AuthResult>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IRefreshTokenStore _refreshTokenStore;

    public LoginWithGoogleCommandHandler(
        IIdentityService identityService,
        IJwtTokenGenerator tokenGenerator,
        IRefreshTokenStore refreshTokenStore)
    {
        _identityService = identityService;
        _tokenGenerator = tokenGenerator;
        _refreshTokenStore = refreshTokenStore;
    }

    public async Task<AuthResult> Handle(LoginWithGoogleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _identityService.FindOrCreateByExternalLoginAsync(
                provider: "Google",
                providerKey: request.Sub,
                email: request.Email,
                displayName: request.DisplayName,
                tenantId: request.TenantId,
                cancellationToken: cancellationToken);

            var response = await AuthTokenIssuer.IssueAsync(
                user,
                _tokenGenerator,
                _refreshTokenStore,
                cancellationToken);

            return AuthResult.Success(response);
        }
        catch (Exception ex)
        {
            return AuthResult.Fail(ex.Message);
        }
    }
}
