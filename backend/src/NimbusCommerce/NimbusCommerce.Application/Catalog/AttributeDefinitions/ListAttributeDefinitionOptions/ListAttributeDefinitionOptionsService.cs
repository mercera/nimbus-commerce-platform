using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Interfaces;

namespace NimbusCommerce.Application.Catalog.AttributeDefinitions.ListAttributeDefinitionOptions;

internal sealed class ListAttributeDefinitionOptionsService : IListAttributeDefinitionOptionsService
{
    private readonly IAttributeDefinitionStore _attributeDefinitionStore;
    private readonly ICurrentUser _currentUser;

    public ListAttributeDefinitionOptionsService(IAttributeDefinitionStore attributeDefinitionStore, ICurrentUser currentUser)
    {
        _attributeDefinitionStore = attributeDefinitionStore;
        _currentUser = currentUser;
    }

    public Task<IReadOnlyList<AttributeDefinitionOption>> ListAsync(CancellationToken cancellationToken) =>
        _attributeDefinitionStore.ListActiveOptionsAsync(_currentUser.RequireUserId(), cancellationToken);
}
