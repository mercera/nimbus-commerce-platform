using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.AttributeDefinitions.GetAttributeDefinition;

internal sealed class GetAttributeDefinitionService : IGetAttributeDefinitionService
{
    private readonly IAttributeDefinitionStore _attributeDefinitionStore;
    private readonly ICurrentUser _currentUser;

    public GetAttributeDefinitionService(IAttributeDefinitionStore attributeDefinitionStore, ICurrentUser currentUser)
    {
        _attributeDefinitionStore = attributeDefinitionStore;
        _currentUser = currentUser;
    }

    public async Task<OperationResult<AttributeDefinitionDetail>> GetAsync(Guid attributeDefinitionId, CancellationToken cancellationToken)
    {
        var detail = await _attributeDefinitionStore.GetDetailAsync(_currentUser.RequireUserId(), attributeDefinitionId, cancellationToken);

        return detail is null
            ? OperationResult<AttributeDefinitionDetail>.NotFound("Attribute definition not found.")
            : OperationResult<AttributeDefinitionDetail>.Success(detail);
    }
}
