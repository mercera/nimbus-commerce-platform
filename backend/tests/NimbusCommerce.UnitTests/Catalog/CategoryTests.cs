using NimbusCommerce.Domain.Catalog;

namespace NimbusCommerce.UnitTests.Catalog;

// Covers only pure Category entity behavior. The "cannot deactivate a category with active
// products" rule requires a database query and is enforced by SetCategoryStatusService, not
// Category itself — see Category.Deactivate's doc comment — so it is not testable here and is
// instead verified by the manual end-to-end script.
public class CategoryTests
{
    private static readonly DateTime UtcNow = new(2026, 8, 14, 12, 0, 0, DateTimeKind.Utc);
    private const string OwnerUserId = "owner-1";

    [Fact]
    public void Create_SetsDefaultsAndAuditFields()
    {
        var category = Category.Create(OwnerUserId, "  Laptops  ", "Portable computers", OwnerUserId, UtcNow);

        Assert.NotEqual(Guid.Empty, category.Id);
        Assert.Equal(OwnerUserId, category.OwnerUserId);
        Assert.Equal("Laptops", category.Name);
        Assert.Equal("Portable computers", category.Description);
        Assert.True(category.IsActive);
        Assert.Equal(UtcNow, category.CreatedAtUtc);
        Assert.Equal(OwnerUserId, category.CreatedByUserId);
        Assert.Null(category.UpdatedAtUtc);
        Assert.Null(category.UpdatedByUserId);
        Assert.Empty(category.Products);
        Assert.Empty(category.AttributeConfigurations);
    }

    [Fact]
    public void Create_TrimsName()
    {
        var category = Category.Create(OwnerUserId, "  Notebooks  ", null, OwnerUserId, UtcNow);

        Assert.Equal("Notebooks", category.Name);
    }

    [Fact]
    public void Rename_TrimsAndUpdatesNameAndAuditFields()
    {
        var category = Category.Create(OwnerUserId, "Laptops", null, OwnerUserId, UtcNow);
        var updatedAt = UtcNow.AddDays(1);

        category.Rename("  Notebooks  ", "editor-1", updatedAt);

        Assert.Equal("Notebooks", category.Name);
        Assert.Equal(updatedAt, category.UpdatedAtUtc);
        Assert.Equal("editor-1", category.UpdatedByUserId);
    }

    [Fact]
    public void UpdateDescription_ReplacesDescription()
    {
        var category = Category.Create(OwnerUserId, "Laptops", "Old description", OwnerUserId, UtcNow);

        category.UpdateDescription("New description", OwnerUserId, UtcNow.AddMinutes(5));

        Assert.Equal("New description", category.Description);
    }

    [Fact]
    public void UpdateDescription_Null_ClearsDescription()
    {
        var category = Category.Create(OwnerUserId, "Laptops", "Old description", OwnerUserId, UtcNow);

        category.UpdateDescription(null, OwnerUserId, UtcNow.AddMinutes(5));

        Assert.Null(category.Description);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalseAndUpdatesAuditFields()
    {
        var category = Category.Create(OwnerUserId, "Laptops", null, OwnerUserId, UtcNow);
        var updatedAt = UtcNow.AddMinutes(1);

        category.Deactivate("editor-1", updatedAt);

        Assert.False(category.IsActive);
        Assert.Equal(updatedAt, category.UpdatedAtUtc);
        Assert.Equal("editor-1", category.UpdatedByUserId);
    }

    [Fact]
    public void Activate_SetsIsActiveTrue()
    {
        var category = Category.Create(OwnerUserId, "Laptops", null, OwnerUserId, UtcNow);
        category.Deactivate(OwnerUserId, UtcNow.AddMinutes(1));

        category.Activate(OwnerUserId, UtcNow.AddMinutes(2));

        Assert.True(category.IsActive);
    }
}
