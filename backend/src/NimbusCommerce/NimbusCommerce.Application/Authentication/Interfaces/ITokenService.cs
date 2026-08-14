namespace NimbusCommerce.Application.Authentication.Interfaces;

/// <summary>
/// Generates and hashes authentication tokens. Contains no persistence or identity logic —
/// see <see cref="IRefreshTokenStore"/> for persistence and <see cref="IIdentityService"/> for
/// credential/identity operations.
/// </summary>
public interface ITokenService
{
    AccessToken GenerateAccessToken(string userId, string email, IReadOnlyList<string> roles);

    GeneratedRefreshToken GenerateRefreshToken();

    /// <summary>
    /// Hashes a raw refresh token presented by a client, so it can be matched against the stored
    /// hash. Produces the same value as <see cref="GeneratedRefreshToken.TokenHash"/> for the
    /// corresponding <see cref="GeneratedRefreshToken.RawToken"/>.
    /// </summary>
    string HashRefreshToken(string rawToken);
}

public sealed record AccessToken(string Token, DateTime ExpiresAtUtc);

/// <summary>
/// A freshly generated refresh token. <see cref="RawToken"/> is handed to the caller exactly once,
/// to be delivered to the client; it is never persisted. Only <see cref="TokenHash"/> is stored.
/// </summary>
public sealed record GeneratedRefreshToken(string RawToken, string TokenHash, DateTime ExpiresAtUtc);
