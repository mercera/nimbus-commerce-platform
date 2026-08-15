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

    /// <summary>
    /// Associates an AttributeDefinition with this category. The caller (AddCategoryAttributeService)
    /// must load AttributeConfigurations first and is expected to have already checked for a
    /// duplicate and validated the target definition exists/is active/is owned by the caller —
    /// the InvalidOperationException here guards a true invariant violation, not a reachable
    /// user-facing path, the same trust relationship ICurrentUser.RequireUserId() has with
    /// [Authorize].
    /// </summary>
    public void AddAttributeConfiguration(Guid attributeDefinitionId, bool isRequired, string userId, DateTime utcNow)
    {
        if (_attributeConfigurations.Any(ac => ac.AttributeDefinitionId == attributeDefinitionId))
        {
            throw new InvalidOperationException(
                $"Category {Id} is already configured with attribute definition {attributeDefinitionId}.");
        }

        _attributeConfigurations.Add(CategoryAttributeDefinition.Create(Id, attributeDefinitionId, isRequired));
        MarkUpdated(userId, utcNow);
    }

    public void SetAttributeRequired(Guid attributeDefinitionId, bool isRequired, string userId, DateTime utcNow)
    {
        var configuration = _attributeConfigurations.SingleOrDefault(ac => ac.AttributeDefinitionId == attributeDefinitionId)
            ?? throw new InvalidOperationException(
                $"Category {Id} has no configuration for attribute definition {attributeDefinitionId}.");

        configuration.SetRequired(isRequired);
        MarkUpdated(userId, utcNow);
    }

    public void RemoveAttributeConfiguration(Guid attributeDefinitionId, string userId, DateTime utcNow)
    {
        var configuration = _attributeConfigurations.SingleOrDefault(ac => ac.AttributeDefinitionId == attributeDefinitionId)
            ?? throw new InvalidOperationException(
                $"Category {Id} has no configuration for attribute definition {attributeDefinitionId}.");

        _attributeConfigurations.Remove(configuration);
        MarkUpdated(userId, utcNow);
    }
}
