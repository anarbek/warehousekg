using MediatR;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Auth.Dtos;

namespace WarehouseKG.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResult>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IRefreshTokenStore _refreshTokenStore;

    public RefreshTokenCommandHandler(
        IIdentityService identityService,
        IJwtTokenGenerator tokenGenerator,
        IRefreshTokenStore refreshTokenStore)
    {
        _identityService = identityService;
        _tokenGenerator = tokenGenerator;
        _refreshTokenStore = refreshTokenStore;
    }

    public async Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Consuming revokes the presented token (single-use rotation).
        var userId = await _refreshTokenStore.ConsumeAsync(request.RefreshToken, cancellationToken);
        if (userId is null)
        {
            return AuthResult.Fail("The refresh token is invalid or has expired.");
        }

        var user = await _identityService.FindByIdAsync(userId.Value, cancellationToken);
        if (user is null)
        {
            return AuthResult.Fail("The refresh token is invalid or has expired.");
        }

        var response = await AuthTokenIssuer.IssueAsync(
            user,
            _tokenGenerator,
            _refreshTokenStore,
            cancellationToken);

        return AuthResult.Success(response);
    }
}
