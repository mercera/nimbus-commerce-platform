using NimbusCommerce.Domain.Common;

namespace NimbusCommerce.Domain.Catalog;

/// <summary>
/// Schema shape only for this milestone (Product Catalogue Foundations &amp; Categories) — no
/// factory method, mutators, or business rules yet. Exists so the Products/CategoryAttributeDefinitions
/// tables can be created now, as part of one migration with the rest of the catalogue schema.
/// Behavior is added in the Products milestone (see project-journal.md).
/// </summary>
public sealed class Product : AuditableEntity
{
    private readonly List<ProductImage> _images = [];
    private readonly List<ProductAttributeValue> _attributeValues = [];

    private Product()
    {
        // EF Core materialization only.
    }

    public Guid Id { get; private set; }

    public string OwnerUserId { get; private set; } = string.Empty;

    public string Sku { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? DescriptionHtml { get; private set; }

    public decimal Price { get; private set; }

    public Guid CategoryId { get; private set; }

    public Category Category { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<ProductImage> Images => _images;

    public IReadOnlyCollection<ProductAttributeValue> AttributeValues => _attributeValues;
}
