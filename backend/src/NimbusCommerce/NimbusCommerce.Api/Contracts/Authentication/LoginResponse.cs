namespace NimbusCommerce.Api.Contracts.Authentication;

/// <summary>
/// Successful login payload. The refresh token is deliberately absent — it is delivered
/// as an HttpOnly cookie so it is never readable by browser script.
/// </summary>
public sealed record LoginResponse(string AccessToken, DateTime ExpiresAtUtc);
