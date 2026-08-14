namespace NimbusCommerce.Domain.Catalog;

/// <summary>
/// Schema shape only for this milestone — no factory or the primary-image/ordering rules yet.
/// Those are added in the Product Images milestone (see project-journal.md). No UpdatedAtUtc/
/// UpdatedByUserId: an image is replaced (deleted and re-added), never edited in place, so it
/// does not inherit AuditableEntity.
/// </summary>
public sealed class ProductImage
{
    private ProductImage()
    {
        // EF Core materialization only.
    }

    public Guid Id { get; private set; }

    public Guid ProductId { get; private set; }

    public Product Product { get; private set; } = null!;

    public string StorageKey { get; private set; } = string.Empty;

    public string FileName { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public long SizeBytes { get; private set; }

    public string? AltText { get; private set; }

    public bool IsPrimary { get; private set; }

    public int DisplayOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public string CreatedByUserId { get; private set; } = string.Empty;
}
