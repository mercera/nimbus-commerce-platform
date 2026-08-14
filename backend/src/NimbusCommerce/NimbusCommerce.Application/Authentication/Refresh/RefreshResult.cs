namespace NimbusCommerce.Application.Authentication.Refresh;

/// <summary>
/// Outcome of a refresh attempt. On failure every field beyond <see cref="Succeeded"/> is null and
/// no cause is reported — unknown, expired, revoked (reused), and deactivated-user must all be
/// indistinguishable to the caller, so a stolen token cannot be probed for its state.
/// </summary>
public sealed record RefreshResult(
    bool Succeeded,
    string? AccessToken,
    DateTime? AccessTokenExpiresAtUtc,
    string? RefreshToken,
    DateTime? RefreshTokenExpiresAtUtc)
{
    public static RefreshResult Success(string accessToken, DateTime accessTokenExpiresAtUtc, string refreshToken, DateTime refreshTokenExpiresAtUtc) =>
        new(true, accessToken, accessTokenExpiresAtUtc, refreshToken, refreshTokenExpiresAtUtc);

    public static RefreshResult Failure() => new(false, null, null, null, null);
}
