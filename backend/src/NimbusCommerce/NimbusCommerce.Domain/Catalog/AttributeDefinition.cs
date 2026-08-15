using NimbusCommerce.Domain.Common;

namespace NimbusCommerce.Domain.Catalog;

/// <summary>
/// Note there is no IsRequired here — requiredness is a property of the
/// CategoryAttributeDefinition relationship, not of the definition itself. ProductAttributeValue
/// remains schema-only until the Products milestone.
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

    public static AttributeDefinition Create(string ownerUserId, string name, AttributeDataType dataType, string userId, DateTime utcNow)
    {
        var attributeDefinition = new AttributeDefinition
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerUserId,
            Name = name.Trim(),
            DataType = dataType,
            IsActive = true
        };

        attributeDefinition.MarkCreated(userId, utcNow);

        return attributeDefinition;
    }

    /// <summary>
    /// Renames only. DataType is fixed at creation — once Products/ProductAttributeValue exist,
    /// changing a definition's type has no sane migration story for already-stored values, so
    /// there is deliberately no mutator for it.
    /// </summary>
    public void Rename(string name, string userId, DateTime utcNow)
    {
        Name = name.Trim();
        MarkUpdated(userId, utcNow);
    }

    public void Activate(string userId, DateTime utcNow)
    {
        IsActive = true;
        MarkUpdated(userId, utcNow);
    }

    /// <summary>
    /// Pure state transition, no query-dependent guard — unlike Category.Deactivate, nothing
    /// downstream reads IsActive at runtime except the "must be active to associate" check in
    /// AddCategoryAttributeService.
    /// </summary>
    public void Deactivate(string userId, DateTime utcNow)
    {
        IsActive = false;
        MarkUpdated(userId, utcNow);
    }
}
