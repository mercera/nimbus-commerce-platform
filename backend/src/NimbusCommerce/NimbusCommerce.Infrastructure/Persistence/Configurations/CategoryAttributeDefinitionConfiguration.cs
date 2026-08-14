using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NimbusCommerce.Domain.Catalog;

namespace NimbusCommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Schema for the Category/AttributeDefinition join lands now, alongside the rest of the
/// catalogue schema. Association/validation behavior is not implemented until the
/// Category/AttributeDefinition configuration milestone — see project-journal.md.
/// </summary>
public sealed class CategoryAttributeDefinitionConfiguration : IEntityTypeConfiguration<CategoryAttributeDefinition>
{
    public void Configure(EntityTypeBuilder<CategoryAttributeDefinition> builder)
    {
        builder.ToTable("CategoryAttributeDefinitions");

        // Composite key: a duplicate (Category, AttributeDefinition) association is impossible
        // by construction, rather than merely prevented by an extra unique index.
        builder.HasKey(cad => new { cad.CategoryId, cad.AttributeDefinitionId });

        builder.Property(cad => cad.IsRequired)
            .HasDefaultValue(false);

        // Cascade: the configuration is meaningless without its category, and a category can
        // only be deleted once it holds zero products anyway.
        builder.HasOne(cad => cad.Category)
            .WithMany(c => c.AttributeConfigurations)
            .HasForeignKey(cad => cad.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // NoAction: deleting an AttributeDefinition must never silently unconfigure a category.
        // The Attribute Definitions milestone's delete guard checks this table before allowing a
        // delete to proceed.
        builder.HasOne(cad => cad.AttributeDefinition)
            .WithMany(a => a.CategoryAssociations)
            .HasForeignKey(cad => cad.AttributeDefinitionId)
            .OnDelete(DeleteBehavior.NoAction);

        // Powers "which categories use this definition?" — read on every attribute-definition
        // delete/deactivate.
        builder.HasIndex(cad => cad.AttributeDefinitionId);
    }
}
