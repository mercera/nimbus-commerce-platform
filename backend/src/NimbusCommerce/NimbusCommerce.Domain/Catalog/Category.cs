using NimbusCommerce.Domain.Common;

namespace NimbusCommerce.Domain.Catalog;

public sealed class Category : AuditableEntity
{
    private readonly List<Product> _products = [];
    private readonly List<CategoryAttributeDefinition> _attributeConfigurations = [];

    private Category()
    {
        // EF Core materialization only.
    }

    public Guid Id { get; private set; }

    public string OwnerUserId { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<Product> Products => _products;

    /// <summary>
    /// Which AttributeDefinitions apply to this category and whether each is required.
    /// Empty in this milestone — populated starting in the Category/AttributeDefinition
    /// configuration milestone (see project-journal.md).
    /// </summary>
    public IReadOnlyCollection<CategoryAttributeDefinition> AttributeConfigurations => _attributeConfigurations;

    public static Category Create(string ownerUserId, string name, string? description, string userId, DateTime utcNow)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerUserId,
            Name = name.Trim(),
            Description = description,
            IsActive = true
        };

        category.MarkCreated(userId, utcNow);

        return category;
    }

    public void Rename(string name, string userId, DateTime utcNow)
    {
        Name = name.Trim();
        MarkUpdated(userId, utcNow);
    }

    public void UpdateDescription(string? description, string userId, DateTime utcNow)
    {
        Description = description;
        MarkUpdated(userId, utcNow);
    }

    public void Activate(string userId, DateTime utcNow)
    {
        IsActive = true;
        MarkUpdated(userId, utcNow);
    }

    /// <summary>
    /// Performs the state transition only. Whether deactivation is allowed (the category must
    /// hold no Active Products) requires a database query and is enforced by the caller
    /// (SetCategoryStatusService) before this method is invoked.
    /// </summary>
    public void Deactivate(string userId, DateTime utcNow)
    {
        IsActive = false;
        MarkUpdated(userId, utcNow);
    }
}
