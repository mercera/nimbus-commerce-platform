using Microsoft.Extensions.Logging;
using NimbusCommerce.Application.Authentication.Interfaces;

namespace NimbusCommerce.Application.Authentication.Logout;

internal sealed class LogoutService : ILogoutService
{
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly ILogger<LogoutService> _logger;

    public LogoutService(ITokenService tokenService, IRefreshTokenStore refreshTokenStore, ILogger<LogoutService> logger)
    {
        _tokenService = tokenService;
        _refreshTokenStore = refreshTokenStore;
        _logger = logger;
    }

    public async Task LogoutAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var presentedHash = _tokenService.HashRefreshToken(refreshToken);

        var revoked = await _refreshTokenStore.RevokeActiveByHashAsync(presentedHash);
        if (!revoked)
        {
            // Not an error: unknown, already-revoked, and expired tokens are all indistinguishable
            // here by design, and logout is idempotent regardless. No token material is logged.
            _logger.LogInformation("Logout presented a refresh token that was not active.");
        }
    }
}
