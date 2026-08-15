using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Interfaces;
using NimbusCommerce.Application.Common.Models;
using NimbusCommerce.Domain.Catalog;

namespace NimbusCommerce.Application.Catalog.AttributeDefinitions.CreateAttributeDefinition;

internal sealed class CreateAttributeDefinitionService : ICreateAttributeDefinitionService
{
    private readonly IAttributeDefinitionStore _attributeDefinitionStore;
    private readonly ICurrentUser _currentUser;

    public CreateAttributeDefinitionService(IAttributeDefinitionStore attributeDefinitionStore, ICurrentUser currentUser)
    {
        _attributeDefinitionStore = attributeDefinitionStore;
        _currentUser = currentUser;
    }

    public async Task<OperationResult<AttributeDefinitionDetail>> CreateAsync(CreateAttributeDefinitionRequest request, CancellationToken cancellationToken)
    {
        var ownerUserId = _currentUser.RequireUserId();
        var name = request.Name.Trim();

        // Pre-check for a clear error message; the database's unique index on (OwnerUserId, Name)
        // is the actual guarantee against a concurrent duplicate create — same accepted
        // check-then-insert race as CreateCategoryService.
        if (await _attributeDefinitionStore.ExistsWithNameAsync(ownerUserId, name, excludingAttributeDefinitionId: null, cancellationToken))
        {
            return OperationResult<AttributeDefinitionDetail>.Conflict($"An attribute definition named '{name}' already exists.");
        }

        var attributeDefinition = AttributeDefinition.Create(ownerUserId, name, request.DataType, ownerUserId, DateTime.UtcNow);

        _attributeDefinitionStore.Add(attributeDefinition);
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
