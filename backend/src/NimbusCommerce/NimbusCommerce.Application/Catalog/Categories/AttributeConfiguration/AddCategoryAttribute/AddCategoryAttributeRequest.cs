using System.ComponentModel.DataAnnotations;

namespace NimbusCommerce.Application.Catalog.Categories.AttributeConfiguration.AddCategoryAttribute;

public sealed class AddCategoryAttributeRequest
{
    // Guid? rather than Guid: [Required] on a non-nullable value type does not reject an omitted
    // field (it silently binds to Guid.Empty), so an omitted AttributeDefinitionId would have
    // fallen through to a 404 ("attribute definition not found") instead of the 400 an omitted
    // required field should produce. Nullable makes "missing" a distinct, correctly-rejected state.
    [Required]
    public Guid? AttributeDefinitionId { get; set; }

    public bool IsRequired { get; set; }
}
