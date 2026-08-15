using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.AttributeDefinitions.ListAttributeDefinitions;

public sealed class ListAttributeDefinitionsRequest : PagedQuery
{
    public string? Search { get; set; }

    public bool? IsActive { get; set; }
}
