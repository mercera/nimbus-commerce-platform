using System.ComponentModel.DataAnnotations;

namespace NimbusCommerce.Application.Catalog.AttributeDefinitions.UpdateAttributeDefinition;

/// <summary>
/// Renames only. DataType is immutable after creation — see AttributeDefinition.Rename's doc
/// comment — so it is deliberately not a field here.
/// </summary>
public sealed class UpdateAttributeDefinitionRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
