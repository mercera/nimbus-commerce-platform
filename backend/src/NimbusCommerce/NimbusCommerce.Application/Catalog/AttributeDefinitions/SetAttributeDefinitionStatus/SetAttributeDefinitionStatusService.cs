using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.AttributeDefinitions.SetAttributeDefinitionStatus;

internal sealed class SetAttributeDefinitionStatusService : ISetAttributeDefinitionStatusService
{
    private readonly IAttributeDefinitionStore _attributeDefinitionStore;
    private readonly ICurrentUser _currentUser;

    public SetAttributeDefinitionStatusService(IAttributeDefinitionStore attributeDefinitionStore, ICurrentUser currentUser)
    {
        _attributeDefinitionStore = attributeDefinitionStore;
        _currentUser = currentUser;
    }

    public async Task<OperationResult> SetStatusAsync(Guid attributeDefinitionId, bool isActive, CancellationToken cancellationToken)
    {
        var ownerUserId = _currentUser.RequireUserId();

        var attributeDefinition = await _attributeDefinitionStore.FindByIdAsync(ownerUserId, attributeDefinitionId, cancellationToken);
        if (attributeDefinition is null)
        {
            return OperationResult.NotFound("Attribute definition not found.");
        }

        var utcNow = DateTime.UtcNow;

        if (isActive)
        {
            attributeDefinition.Activate(ownerUserId, utcNow);
        }
        else
        {
            attributeDefinition.Deactivate(ownerUserId, utcNow);
        }

        await _attributeDefinitionStore.SaveChangesAsync(cancellationToken);

        return OperationResult.Success();
    }
}
