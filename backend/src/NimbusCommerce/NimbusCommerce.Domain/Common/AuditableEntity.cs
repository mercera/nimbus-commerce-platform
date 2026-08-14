namespace NimbusCommerce.Domain.Common;

public abstract class AuditableEntity
{
    public DateTime CreatedAtUtc { get; private set; }

    public string CreatedByUserId { get; private set; } = string.Empty;

    public DateTime? UpdatedAtUtc { get; private set; }

    public string? UpdatedByUserId { get; private set; }

    protected void MarkCreated(string userId, DateTime utcNow)
    {
        CreatedAtUtc = utcNow;
        CreatedByUserId = userId;
    }

    protected void MarkUpdated(string userId, DateTime utcNow)
    {
        UpdatedAtUtc = utcNow;
        UpdatedByUserId = userId;
    }
}
