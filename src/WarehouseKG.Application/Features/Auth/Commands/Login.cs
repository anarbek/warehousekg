using MediatR;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Auth.Dtos;

namespace WarehouseKG.Application.Features.Auth.Commands;

public record LoginCommand(string UserName, string Password) : IRequest<AuthResult>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IRefreshTokenStore _refreshTokenStore;

    public LoginCommandHandler(
        IIdentityService identityService,
        IJwtTokenGenerator tokenGenerator,
        IRefreshTokenStore refreshTokenStore)
    {
        _identityService = identityService;
        _tokenGenerator = tokenGenerator;
        _refreshTokenStore = refreshTokenStore;
    }

    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.ValidateCredentialsAsync(
            request.UserName,
            request.Password,
            cancellationToken);

        if (user is null)
        {
            return AuthResult.Fail("Invalid username or password.");
        }

        var response = await AuthTokenIssuer.IssueAsync(
            user,
            _tokenGenerator,
            _refreshTokenStore,
            cancellationToken);

        return AuthResult.Success(response);
    }
}
