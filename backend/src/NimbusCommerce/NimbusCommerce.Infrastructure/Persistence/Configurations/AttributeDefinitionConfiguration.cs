using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NimbusCommerce.Domain.Catalog;
using NimbusCommerce.Infrastructure.Identity;

namespace NimbusCommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Schema for AttributeDefinitions lands now, alongside the rest of the catalogue schema.
/// AttributeDefinition's own use-case behavior is not implemented until the Attribute Definitions
/// milestone — see project-journal.md. Deliberately no IsRequired column: requiredness is a
/// property of the CategoryAttributeDefinition relationship, not of the definition itself.
/// </summary>
public sealed class AttributeDefinitionConfiguration : IEntityTypeConfiguration<AttributeDefinition>
{
    public void Configure(EntityTypeBuilder<AttributeDefinition> builder)
    {
        builder.ToTable("AttributeDefinitions");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.OwnerUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.DataType)
            .IsRequired();

        builder.Property(a => a.IsActive)
            .HasDefaultValue(true);

        builder.Property(a => a.CreatedByUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(a => a.UpdatedByUserId)
            .HasMaxLength(450);

        builder.HasIndex(a => new { a.OwnerUserId, a.Name }).IsUnique();
        builder.HasIndex(a => new { a.OwnerUserId, a.IsActive });

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(a => a.OwnerUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
