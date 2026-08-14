using System.ComponentModel.DataAnnotations;

namespace NimbusCommerce.Application.Catalog.Categories.UpdateCategory;

public sealed class UpdateCategoryRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }
}
