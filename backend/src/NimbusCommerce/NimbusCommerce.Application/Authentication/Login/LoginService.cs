using NimbusCommerce.Application.Authentication.Interfaces;

namespace NimbusCommerce.Application.Authentication.Login;

internal sealed class LoginService : ILoginService
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenStore _refreshTokenStore;

    public LoginService(IIdentityService identityService, ITokenService tokenService, IRefreshTokenStore refreshTokenStore)
    {
        _identityService = identityService;
        _tokenService = tokenService;
        _refreshTokenStore = refreshTokenStore;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, string? deviceName)
    {
        var userId = await _identityService.ValidateCredentialsAsync(request.Email, request.Password);
        if (userId is null)
        {
            return LoginResult.Failure();
        }

        var roles = await _identityService.GetRolesAsync(userId);
        var accessToken = _tokenService.GenerateAccessToken(userId, request.Email, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        await _refreshTokenStore.SaveAsync(userId, refreshToken.TokenHash, refreshToken.ExpiresAtUtc, deviceName);

        return LoginResult.Success(accessToken.Token, accessToken.ExpiresAtUtc, refreshToken.RawToken, refreshToken.ExpiresAtUtc);
    }
}
