using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.AttributeDefinitions.UpdateAttributeDefinition;

internal sealed class UpdateAttributeDefinitionService : IUpdateAttributeDefinitionService
{
    private readonly IAttributeDefinitionStore _attributeDefinitionStore;
    private readonly ICurrentUser _currentUser;

    public UpdateAttributeDefinitionService(IAttributeDefinitionStore attributeDefinitionStore, ICurrentUser currentUser)
    {
        _attributeDefinitionStore = attributeDefinitionStore;
        _currentUser = currentUser;
    }

    public async Task<OperationResult<AttributeDefinitionDetail>> UpdateAsync(Guid attributeDefinitionId, UpdateAttributeDefinitionRequest request, CancellationToken cancellationToken)
    {
        var ownerUserId = _currentUser.RequireUserId();

        var attributeDefinition = await _attributeDefinitionStore.FindByIdAsync(ownerUserId, attributeDefinitionId, cancellationToken);
        if (attributeDefinition is null)
        {
            return OperationResult<AttributeDefinitionDetail>.NotFound("Attribute definition not found.");
        }

        var name = request.Name.Trim();

        if (!string.Equals(name, attributeDefinition.Name, StringComparison.Ordinal) &&
            await _attributeDefinitionStore.ExistsWithNameAsync(ownerUserId, name, attributeDefinitionId, cancellationToken))
        {
            return OperationResult<AttributeDefinitionDetail>.Conflict($"An attribute definition named '{name}' already exists.");
        }

        attributeDefinition.Rename(name, ownerUserId, DateTime.UtcNow);

        await _attributeDefinitionStore.SaveChangesAsync(cancellationToken);

        return OperationResult<AttributeDefinitionDetail>.Success(new AttributeDefinitionDetail(
            attributeDefinition.Id,
            attributeDefinition.Name,
            attributeDefinition.DataType,
            attributeDefinition.IsActive,
            attributeDefinition.CreatedAtUtc,
            attributeDefinition.CreatedByUserId,
            attributeDefinition.UpdatedAtUtc,
            attributeDefinition.UpdatedByUserId));
    }
}
