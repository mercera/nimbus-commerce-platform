using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NimbusCommerce.Domain.Catalog;
using NimbusCommerce.Infrastructure.Identity;

namespace NimbusCommerce.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.OwnerUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedByUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(c => c.UpdatedByUserId)
            .HasMaxLength(450);

        builder.HasIndex(c => new { c.OwnerUserId, c.Name }).IsUnique();
        builder.HasIndex(c => new { c.OwnerUserId, c.IsActive });

        // FK to AspNetUsers without a navigation property — Domain must not reference
        // ApplicationUser (see Architecture.md, "Entity placement"). NoAction: deleting a user
        // must not silently destroy catalogue history.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.OwnerUserId)
            .OnDelete(DeleteBehavior.NoAction);

        // The Category -> Products relationship is fully configured from Product's side
        // (ProductConfiguration owns the FK); not repeated here to avoid two configurations
        // disagreeing about the same relationship.
    }
}
