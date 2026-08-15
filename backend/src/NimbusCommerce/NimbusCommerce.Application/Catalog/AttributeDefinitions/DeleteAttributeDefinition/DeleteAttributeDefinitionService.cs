using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.AttributeDefinitions.DeleteAttributeDefinition;

internal sealed class DeleteAttributeDefinitionService : IDeleteAttributeDefinitionService
{
    private readonly IAttributeDefinitionStore _attributeDefinitionStore;
    private readonly ICurrentUser _currentUser;

    public DeleteAttributeDefinitionService(IAttributeDefinitionStore attributeDefinitionStore, ICurrentUser currentUser)
    {
        _attributeDefinitionStore = attributeDefinitionStore;
        _currentUser = currentUser;
    }

    public async Task<OperationResult> DeleteAsync(Guid attributeDefinitionId, CancellationToken cancellationToken)
    {
        var ownerUserId = _currentUser.RequireUserId();

        var attributeDefinition = await _attributeDefinitionStore.FindByIdAsync(ownerUserId, attributeDefinitionId, cancellationToken);
        if (attributeDefinition is null)
        {
            return OperationResult.NotFound("Attribute definition not found.");
        }

        // Deleting an AttributeDefinition must never silently unconfigure a category — see
        // CategoryAttributeDefinitionConfiguration's FK comment (NoAction).
        var associationCount = await _attributeDefinitionStore.CountCategoryAssociationsAsync(ownerUserId, attributeDefinitionId, cancellationToken);
        if (associationCount > 0)
        {
            return OperationResult.RuleViolation(
                $"Cannot delete this attribute definition: it is associated with {associationCount} categor{(associationCount == 1 ? "y" : "ies")}. Remove those associations first.");
        }

        _attributeDefinitionStore.Remove(attributeDefinition);
        await _attributeDefinitionStore.SaveChangesAsync(cancellationToken);

        return OperationResult.Success();
    }
}
