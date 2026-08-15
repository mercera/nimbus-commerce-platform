using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.Categories.AttributeConfiguration.SetCategoryAttributeRequired;

internal sealed class SetCategoryAttributeRequiredService : ISetCategoryAttributeRequiredService
{
    private readonly ICategoryStore _categoryStore;
    private readonly ICurrentUser _currentUser;

    public SetCategoryAttributeRequiredService(ICategoryStore categoryStore, ICurrentUser currentUser)
    {
        _categoryStore = categoryStore;
        _currentUser = currentUser;
    }

    public async Task<OperationResult<CategoryAttributeItem>> SetRequiredAsync(Guid categoryId, Guid attributeDefinitionId, SetCategoryAttributeRequiredRequest request, CancellationToken cancellationToken)
    {
        var ownerUserId = _currentUser.RequireUserId();

        var category = await _categoryStore.FindByIdWithAttributeConfigurationsAsync(ownerUserId, categoryId, cancellationToken);
        if (category is null)
        {
            return OperationResult<CategoryAttributeItem>.NotFound("Category not found.");
        }

        var configuration = category.AttributeConfigurations.SingleOrDefault(ac => ac.AttributeDefinitionId == attributeDefinitionId);
        if (configuration is null)
        {
            return OperationResult<CategoryAttributeItem>.NotFound("Category is not configured with this attribute definition.");
        }

        category.SetAttributeRequired(attributeDefinitionId, request.IsRequired, ownerUserId, DateTime.UtcNow);

        await _categoryStore.SaveChangesAsync(cancellationToken);

        return OperationResult<CategoryAttributeItem>.Success(new CategoryAttributeItem(
            configuration.AttributeDefinitionId,
            configuration.AttributeDefinition.Name,
            configuration.AttributeDefinition.DataType,
            request.IsRequired));
    }
}
