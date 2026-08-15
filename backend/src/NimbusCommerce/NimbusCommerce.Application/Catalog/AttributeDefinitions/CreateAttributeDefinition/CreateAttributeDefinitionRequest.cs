using System.ComponentModel.DataAnnotations;
using NimbusCommerce.Domain.Catalog;

namespace NimbusCommerce.Application.Catalog.AttributeDefinitions.CreateAttributeDefinition;

public sealed class CreateAttributeDefinitionRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EnumDataType(typeof(AttributeDataType))]
    public AttributeDataType DataType { get; set; }
}
