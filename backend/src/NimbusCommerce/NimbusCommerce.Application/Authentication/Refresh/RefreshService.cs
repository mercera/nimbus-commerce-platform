using Microsoft.Extensions.Logging;
using NimbusCommerce.Application.Authentication.Interfaces;

namespace NimbusCommerce.Application.Authentication.Refresh;

internal sealed class RefreshService : IRefreshService
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly ILogger<RefreshService> _logger;

    public RefreshService(
        IIdentityService identityService,
        ITokenService tokenService,
        IRefreshTokenStore refreshTokenStore,
        ILogger<RefreshService> logger)
    {
        _identityService = identityService;
        _tokenService = tokenService;
        _refreshTokenStore = refreshTokenStore;
        _logger = logger;
    }

    public async Task<RefreshResult> RefreshAsync(string refreshToken, string? deviceName)
    {

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return RefreshResult.Failure();
        }

        var presentedHash = _tokenService.HashRefreshToken(refreshToken);

        var stored = await _refreshTokenStore.FindByHashAsync(presentedHash);

        if (stored is null)
        {
            return RefreshResult.Failure();
        }

        // Reuse detection must precede the expiry check: replaying a token that was already
        // rotated away is evidence of theft whether or not it has since lapsed.
        if (stored.RevokedAtUtc is not null)
        {
            _logger.LogWarning(
                "Refresh token reuse detected for user {UserId}; revoking all active tokens.",
                stored.UserId);
            await _refreshTokenStore.RevokeAllActiveForUserAsync(stored.UserId);
            return RefreshResult.Failure();
        }

        if (stored.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return RefreshResult.Failure();
        }

        var user = await _identityService.GetActiveUserAsync(stored.UserId);
        if (user is null)
        {
            return RefreshResult.Failure();
        }

        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Persist before issuing: never hand out an access token for a rotation that did not commit.
        await _refreshTokenStore.RotateAsync(
            presentedHash, newRefreshToken.TokenHash, newRefreshToken.ExpiresAtUtc, deviceName);

        var accessToken = _tokenService.GenerateAccessToken(user.UserId, user.Email, user.Roles);

        return RefreshResult.Success(
            accessToken.Token, accessToken.ExpiresAtUtc,
            newRefreshToken.RawToken, newRefreshToken.ExpiresAtUtc);
    }
}
