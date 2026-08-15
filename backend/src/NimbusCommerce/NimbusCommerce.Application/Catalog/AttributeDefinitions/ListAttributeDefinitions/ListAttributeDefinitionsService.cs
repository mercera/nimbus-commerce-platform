using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.AttributeDefinitions.ListAttributeDefinitions;

internal sealed class ListAttributeDefinitionsService : IListAttributeDefinitionsService
{
    private readonly IAttributeDefinitionStore _attributeDefinitionStore;
    private readonly ICurrentUser _currentUser;

    public ListAttributeDefinitionsService(IAttributeDefinitionStore attributeDefinitionStore, ICurrentUser currentUser)
    {
        _attributeDefinitionStore = attributeDefinitionStore;
        _currentUser = currentUser;
    }

    public Task<PagedResult<AttributeDefinitionListItem>> ListAsync(ListAttributeDefinitionsRequest request, CancellationToken cancellationToken)
    {
        var query = new AttributeDefinitionQuery(request.Search, request.IsActive, request.Page, request.PageSize);

        return _attributeDefinitionStore.ListAsync(_currentUser.RequireUserId(), query, cancellationToken);
    }
}
