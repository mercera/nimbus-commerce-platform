using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.Categories.AttributeConfiguration.AddCategoryAttribute;

internal sealed class AddCategoryAttributeService : IAddCategoryAttributeService
{
    private readonly ICategoryStore _categoryStore;
    private readonly IAttributeDefinitionStore _attributeDefinitionStore;
    private readonly ICurrentUser _currentUser;

    public AddCategoryAttributeService(ICategoryStore categoryStore, IAttributeDefinitionStore attributeDefinitionStore, ICurrentUser currentUser)
    {
        _categoryStore = categoryStore;
        _attributeDefinitionStore = attributeDefinitionStore;
        _currentUser = currentUser;
    }

    public async Task<OperationResult<CategoryAttributeItem>> AddAsync(Guid categoryId, AddCategoryAttributeRequest request, CancellationToken cancellationToken)
    {
        var ownerUserId = _currentUser.RequireUserId();

        var category = await _categoryStore.FindByIdWithAttributeConfigurationsAsync(ownerUserId, categoryId, cancellationToken);
        if (category is null)
        {
            return OperationResult<CategoryAttributeItem>.NotFound("Category not found.");
        }

        // [ApiController] model validation (Required on Guid?) has already rejected a missing
        // AttributeDefinitionId with 400 before this service runs, so .Value is safe here.
        var attributeDefinitionId = request.AttributeDefinitionId!.Value;

        var attributeDefinition = await _attributeDefinitionStore.GetDetailAsync(ownerUserId, attributeDefinitionId, cancellationToken);
        if (attributeDefinition is null)
        {
            return OperationResult<CategoryAttributeItem>.NotFound("Attribute definition not found.");
        }

        if (!attributeDefinition.IsActive)
        {
            return OperationResult<CategoryAttributeItem>.RuleViolation(
                $"Cannot associate inactive attribute definition '{attributeDefinition.Name}' with a category.");
        }

        // Pre-check against the collection already loaded above — cheaper than a separate store
        // call, unlike the category-name uniqueness check, because FindByIdWithAttributeConfigurationsAsync
        // already brought it into memory. The composite primary key on CategoryAttributeDefinitions
        // remains the actual concurrency backstop, same accepted check-then-insert race as
        // category name uniqueness.
        if (category.AttributeConfigurations.Any(ac => ac.AttributeDefinitionId == attributeDefinitionId))
        {
            return OperationResult<CategoryAttributeItem>.Conflict(
                $"Category '{category.Name}' is already configured with attribute definition '{attributeDefinition.Name}'.");
        }

        category.AddAttributeConfiguration(attributeDefinitionId, request.IsRequired, ownerUserId, DateTime.UtcNow);

        await _categoryStore.SaveChangesAsync(cancellationToken);

        return OperationResult<CategoryAttributeItem>.Success(new CategoryAttributeItem(
            attributeDefinition.Id,
            attributeDefinition.Name,
            attributeDefinition.DataType,
            request.IsRequired));
    }
}
