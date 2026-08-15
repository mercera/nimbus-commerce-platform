namespace NimbusCommerce.Domain.Catalog;

/// <summary>
/// Join between a Category and an AttributeDefinition: whether the definition applies to the
/// category, and whether it is required there. Identified by the composite
/// (CategoryId, AttributeDefinitionId) key rather than a surrogate Id, so a duplicate association
/// is impossible by construction. Part of the Category aggregate — Create/SetRequired are
/// internal, callable only from Category, which is the sole entry point for configuring
/// associations and stamps its own audit fields on every mutation.
/// </summary>
public sealed class CategoryAttributeDefinition
{
    private CategoryAttributeDefinition()
    {
        // EF Core materialization only.
    }

    public Guid CategoryId { get; private set; }

    public Category Category { get; private set; } = null!;

    public Guid AttributeDefinitionId { get; private set; }

    public AttributeDefinition AttributeDefinition { get; private set; } = null!;

    public bool IsRequired { get; private set; }

    internal static CategoryAttributeDefinition Create(Guid categoryId, Guid attributeDefinitionId, bool isRequired) =>
        new()
        {
            CategoryId = categoryId,
            AttributeDefinitionId = attributeDefinitionId,
            IsRequired = isRequired
        };

    internal void SetRequired(bool isRequired)
    {
        IsRequired = isRequired;
    }
}
