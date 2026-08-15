using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NimbusCommerce.Domain.Catalog;
using NimbusCommerce.Infrastructure.Identity;

namespace NimbusCommerce.Infrastructure.Persistence;

public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Category> Categories => Set<Category>();

    // Product, AttributeDefinition, CategoryAttributeDefinition, ProductAttributeValue, and
    // ProductImage are schema-only as of Sprint 4, Milestone 1: their tables, indexes, and
    // constraints exist (created together with Category's in the AddProductCatalog migration),
    // but no use case, store, or endpoint reads or writes through them yet. Behavior is added in
    // each entity's own later milestone — see project-journal.md.
    public DbSet<Product> Products => Set<Product>();

    public DbSet<AttributeDefinition> AttributeDefinitions => Set<AttributeDefinition>();

    public DbSet<CategoryAttributeDefinition> CategoryAttributeDefinitions => Set<CategoryAttributeDefinition>();

    public DbSet<ProductAttributeValue> ProductAttributeValues => Set<ProductAttributeValue>();

    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
