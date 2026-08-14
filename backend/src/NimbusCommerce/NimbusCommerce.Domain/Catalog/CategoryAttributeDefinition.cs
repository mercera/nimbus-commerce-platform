namespace NimbusCommerce.Domain.Catalog;

/// <summary>
/// Join between a Category and an AttributeDefinition: whether the definition applies to the
/// category, and whether it is required there. Schema shape only for this milestone — no
/// association/validation behavior yet. That lands in the Category/AttributeDefinition
/// configuration milestone (see project-journal.md). Identified by the composite
/// (CategoryId, AttributeDefinitionId) key rather than a surrogate Id, so a duplicate association
/// is impossible by construction.
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
}
