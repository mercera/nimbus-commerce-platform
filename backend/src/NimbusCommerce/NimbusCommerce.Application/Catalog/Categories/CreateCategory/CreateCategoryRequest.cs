using System.ComponentModel.DataAnnotations;

namespace NimbusCommerce.Application.Catalog.Categories.CreateCategory;

public sealed class CreateCategoryRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }
}
