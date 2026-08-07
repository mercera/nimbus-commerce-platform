namespace NimbusCommerce.Application.Authentication.Interfaces;

/// <summary>
/// Persists and reads back refresh tokens. Application code never references the underlying entity
/// or DbContext; only primitives cross this boundary.
/// </summary>
public interface IRefreshTokenStore
{
    Task SaveAsync(string userId, string tokenHash, DateTime expiresAtUtc, string? deviceName);

    /// <summary>
    /// Returns the stored token matching <paramref name="tokenHash"/>, or null if no such token
    /// exists. Returns revoked and expired tokens too — deciding whether a token is usable is the
    /// caller's job, because "revoked" and "expired" are handled differently.
    /// </summary>
    Task<StoredRefreshToken?> FindByHashAsync(string tokenHash);

    /// <summary>
    /// Atomically revokes the token identified by <paramref name="currentTokenHash"/>, links it to
    /// its replacement, and inserts the replacement. Both writes commit together or not at all.
    /// </summary>
    Task RotateAsync(string currentTokenHash, string newTokenHash, DateTime newExpiresAtUtc, string? deviceName);

    /// <summary>
    /// Revokes every currently-usable token belonging to <paramref name="userId"/>. Used when a
    /// revoked token is presented again, which indicates the token has leaked.
    /// </summary>
    Task RevokeAllActiveForUserAsync(string userId);
}

/// <summary>
/// A persisted refresh token, reduced to the primitives Application needs to decide whether it may
/// be used. Carries raw timestamps rather than a computed "is usable" flag — the distinction
/// between revoked and expired drives different behavior.
/// </summary>
public sealed record StoredRefreshToken(string UserId, DateTime ExpiresAtUtc, DateTime? RevokedAtUtc);
