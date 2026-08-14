using NimbusCommerce.Domain.Common;

namespace NimbusCommerce.Domain.Catalog;

/// <summary>
/// Schema shape only for this milestone (Product Catalogue Foundations &amp; Categories) — no
/// factory method, mutators, or business rules yet. Exists so the CategoryAttributeDefinitions/
/// ProductAttributeValues tables can be created now, as part of one migration with the rest of
/// the catalogue schema. Behavior is added in the Attribute Definitions milestone (see
/// project-journal.md). Note there is no IsRequired here — requiredness is a property of the
/// CategoryAttributeDefinition relationship, not of the definition itself.
/// </summary>
public sealed class AttributeDefinition : AuditableEntity
{
    private readonly List<CategoryAttributeDefinition> _categoryAssociations = [];
    private readonly List<ProductAttributeValue> _productValues = [];

    private AttributeDefinition()
    {
        // EF Core materialization only.
    }

    public Guid Id { get; private set; }

    public string OwnerUserId { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public AttributeDataType DataType { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<CategoryAttributeDefinition> CategoryAssociations => _categoryAssociations;

    public IReadOnlyCollection<ProductAttributeValue> ProductValues => _productValues;
}
