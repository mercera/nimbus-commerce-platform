using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.Categories.AttributeConfiguration.RemoveCategoryAttribute;

internal sealed class RemoveCategoryAttributeService : IRemoveCategoryAttributeService
{
    private readonly ICategoryStore _categoryStore;
    private readonly ICurrentUser _currentUser;

    public RemoveCategoryAttributeService(ICategoryStore categoryStore, ICurrentUser currentUser)
    {
        _categoryStore = categoryStore;
        _currentUser = currentUser;
    }

    public async Task<OperationResult> RemoveAsync(Guid categoryId, Guid attributeDefinitionId, CancellationToken cancellationToken)
    {
        var ownerUserId = _currentUser.RequireUserId();

        var category = await _categoryStore.FindByIdWithAttributeConfigurationsAsync(ownerUserId, categoryId, cancellationToken);
        if (category is null)
        {
            return OperationResult.NotFound("Category not found.");
        }

        if (category.AttributeConfigurations.All(ac => ac.AttributeDefinitionId != attributeDefinitionId))
        {
            return OperationResult.NotFound("Category is not configured with this attribute definition.");
        }

        category.RemoveAttributeConfiguration(attributeDefinitionId, ownerUserId, DateTime.UtcNow);

        await _categoryStore.SaveChangesAsync(cancellationToken);

        return OperationResult.Success();
    }
}
