using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.Categories.ListCategories;

public sealed class ListCategoriesRequest : PagedQuery
{
    public string? Search { get; set; }

    public bool? IsActive { get; set; }
}
