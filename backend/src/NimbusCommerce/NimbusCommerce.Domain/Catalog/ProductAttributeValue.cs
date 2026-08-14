namespace NimbusCommerce.Domain.Catalog;

/// <summary>
/// Schema shape only for this milestone — no factory, mutators, or type-matching validation yet.
/// Exactly one of the four typed value columns is populated per row (enforced by a database CHECK
/// constraint); which one must match the referenced AttributeDefinition's DataType is a
/// cross-table rule enforced by ProductAttributeValidator in the Products milestone (see
/// project-journal.md).
/// </summary>
public sealed class ProductAttributeValue
{
    private ProductAttributeValue()
    {
        // EF Core materialization only.
    }

    public Guid Id { get; private set; }

    public Guid ProductId { get; private set; }

    public Product Product { get; private set; } = null!;

    public Guid AttributeDefinitionId { get; private set; }

    public AttributeDefinition AttributeDefinition { get; private set; } = null!;

    public string? TextValue { get; private set; }

    public long? NumberValue { get; private set; }

    public decimal? DecimalValue { get; private set; }

    public bool? BooleanValue { get; private set; }
}
