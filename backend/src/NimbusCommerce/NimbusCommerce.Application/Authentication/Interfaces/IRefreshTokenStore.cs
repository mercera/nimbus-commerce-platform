namespace NimbusCommerce.Application.Authentication.Interfaces;

/// <summary>
/// Persists refresh tokens. Application code never references the underlying entity or DbContext;
/// only primitives cross this boundary.
/// </summary>
public interface IRefreshTokenStore
{
    Task SaveAsync(string userId, string tokenHash, DateTime expiresAtUtc, string? deviceName);
}
