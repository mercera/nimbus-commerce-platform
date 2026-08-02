namespace NimbusCommerce.Infrastructure.Identity;

public sealed class RefreshToken
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable label for the device/session this token was issued to (e.g. browser/OS string).
    /// Not a stable device identifier.
    /// </summary>
    public string? DeviceName { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    /// <summary>
    /// Hash of the token that replaced this one during rotation, if any.
    /// </summary>
    public string? ReplacedByTokenHash { get; set; }

    public bool IsActive => RevokedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;
}
