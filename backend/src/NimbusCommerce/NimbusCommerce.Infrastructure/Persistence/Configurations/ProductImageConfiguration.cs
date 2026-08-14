using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NimbusCommerce.Domain.Catalog;

namespace NimbusCommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Schema for ProductImages lands now, alongside the rest of the catalogue schema. Upload,
/// primary-image promotion, and ordering behavior are not implemented until the Product Images
/// milestone — see project-journal.md.
/// </summary>
public sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.StorageKey)
            .IsRequired()
            .HasMaxLength(400);

        builder.Property(i => i.FileName)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(i => i.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.AltText)
            .HasMaxLength(300);

        builder.Property(i => i.CreatedByUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(i => i.StorageKey).IsUnique();
        builder.HasIndex(i => new { i.ProductId, i.DisplayOrder });

        // Filtered unique index: "at most one primary image per product" is a database
        // guarantee, not merely an application-level one. The "at least one when images exist"
        // half cannot be expressed in SQL without a trigger and is an aggregate rule instead.
        builder.HasIndex(i => i.ProductId)
            .IsUnique()
            .HasFilter("[IsPrimary] = 1")
            .HasDatabaseName("IX_ProductImages_ProductId_Primary");

        builder.HasOne(i => i.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
