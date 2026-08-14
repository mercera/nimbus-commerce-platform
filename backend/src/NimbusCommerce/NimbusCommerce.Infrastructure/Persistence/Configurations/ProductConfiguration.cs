using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NimbusCommerce.Domain.Catalog;
using NimbusCommerce.Infrastructure.Identity;

namespace NimbusCommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Full schema for the Products table lands now, alongside the rest of the catalogue schema, in
/// the single AddProductCatalog migration — see project-journal.md. Product's own use-case
/// behavior (create/update/etc.) is not implemented until the Products milestone.
/// </summary>
public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.OwnerUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.DescriptionHtml)
            .HasColumnType("nvarchar(max)");

        // First money column in the solution — decimal(18,2) for a customer-facing selling price.
        builder.Property(p => p.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedByUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(p => p.UpdatedByUserId)
            .HasMaxLength(450);

        builder.HasIndex(p => new { p.OwnerUserId, p.Sku }).IsUnique();
        builder.HasIndex(p => new { p.OwnerUserId, p.CategoryId });
        builder.HasIndex(p => new { p.OwnerUserId, p.IsActive, p.Name });
        builder.HasIndex(p => p.CategoryId);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(p => p.OwnerUserId)
            .OnDelete(DeleteBehavior.NoAction);

        // NoAction, not Cascade: a category is only deletable when it holds zero products (an
        // application-level guard), so this FK is a backstop, never expected to fire a cascade.
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
