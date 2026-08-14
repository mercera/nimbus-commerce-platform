namespace NimbusCommerce.Application.Authentication.Login;

/// <summary>
/// Outcome of a login attempt. On failure, every field beyond <see cref="Succeeded"/> is null —
/// callers must return a single generic failure response regardless of the underlying cause
/// (unknown email, wrong password, inactive account, or lockout) to avoid leaking account existence.
/// </summary>
public sealed record LoginResult(
    bool Succeeded,
    string? AccessToken,
    DateTime? AccessTokenExpiresAtUtc,
    string? RefreshToken,
    DateTime? RefreshTokenExpiresAtUtc)
{
    public static LoginResult Success(string accessToken, DateTime accessTokenExpiresAtUtc, string refreshToken, DateTime refreshTokenExpiresAtUtc) =>
        new(true, accessToken, accessTokenExpiresAtUtc, refreshToken, refreshTokenExpiresAtUtc);

    public static LoginResult Failure() => new(false, null, null, null, null);
}
