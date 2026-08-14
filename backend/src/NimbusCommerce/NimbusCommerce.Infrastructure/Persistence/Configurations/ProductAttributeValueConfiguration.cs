using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NimbusCommerce.Domain.Catalog;

namespace NimbusCommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Schema for ProductAttributeValues lands now, alongside the rest of the catalogue schema.
/// Read/write behavior and type-matching validation are not implemented until the Products
/// milestone — see project-journal.md.
/// </summary>
public sealed class ProductAttributeValueConfiguration : IEntityTypeConfiguration<ProductAttributeValue>
{
    public void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
    {
        // Enforces "exactly one value column is populated" at the database level. It does NOT
        // enforce that the populated column matches the referenced AttributeDefinition's
        // DataType — that is a cross-table rule enforced in the Products milestone.
        builder.ToTable("ProductAttributeValues", table => table.HasCheckConstraint(
            "CK_ProductAttributeValues_ExactlyOneValue",
            "((CASE WHEN [TextValue] IS NULL THEN 0 ELSE 1 END) + " +
            "(CASE WHEN [NumberValue] IS NULL THEN 0 ELSE 1 END) + " +
            "(CASE WHEN [DecimalValue] IS NULL THEN 0 ELSE 1 END) + " +
            "(CASE WHEN [BooleanValue] IS NULL THEN 0 ELSE 1 END)) = 1"));

        builder.HasKey(v => v.Id);

        builder.Property(v => v.TextValue)
            .HasMaxLength(1000);

        // Generic user-defined measurement (screen size, weight), not money — (18,4), not the
        // (18,2) used for Product.Price.
        builder.Property(v => v.DecimalValue)
            .HasPrecision(18, 4);

        builder.HasIndex(v => new { v.ProductId, v.AttributeDefinitionId }).IsUnique();
        builder.HasIndex(v => v.AttributeDefinitionId);

        builder.HasOne(v => v.Product)
            .WithMany(p => p.AttributeValues)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // NoAction: deleting an AttributeDefinition must never silently delete recorded product
        // history. The delete guard checks this table first.
        builder.HasOne(v => v.AttributeDefinition)
            .WithMany(a => a.ProductValues)
            .HasForeignKey(v => v.AttributeDefinitionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
