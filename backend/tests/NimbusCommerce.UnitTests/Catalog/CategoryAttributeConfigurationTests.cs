using NimbusCommerce.Domain.Catalog;

namespace NimbusCommerce.UnitTests.Catalog;

// Covers Category's attribute-configuration mutators (AddAttributeConfiguration/
// SetAttributeRequired/RemoveAttributeConfiguration). These are pure in-memory collection
// operations on an already-loaded Category — the duplicate-association pre-check and the
// "attribute definition must exist/be active" validation that precede these calls live in
// AddCategoryAttributeService and require a database query, so they are verified by the manual
// end-to-end script instead, same boundary CategoryTests documents for the active-product-count
// rule.
public class CategoryAttributeConfigurationTests
{
    private static readonly DateTime UtcNow = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);
    private const string OwnerUserId = "owner-1";

    [Fact]
    public void AddAttributeConfiguration_AddsAssociationAndUpdatesAuditFields()
    {
        var category = Category.Create(OwnerUserId, "Laptops", null, OwnerUserId, UtcNow);
        var attributeDefinitionId = Guid.NewGuid();
        var updatedAt = UtcNow.AddMinutes(1);

        category.AddAttributeConfiguration(attributeDefinitionId, isRequired: true, "editor-1", updatedAt);

        var configuration = Assert.Single(category.AttributeConfigurations);
        Assert.Equal(attributeDefinitionId, configuration.AttributeDefinitionId);
        Assert.True(configuration.IsRequired);
        Assert.Equal(updatedAt, category.UpdatedAtUtc);
        Assert.Equal("editor-1", category.UpdatedByUserId);
    }

    [Fact]
    public void AddAttributeConfiguration_Duplicate_Throws()
    {
        var category = Category.Create(OwnerUserId, "Laptops", null, OwnerUserId, UtcNow);
        var attributeDefinitionId = Guid.NewGuid();
        category.AddAttributeConfiguration(attributeDefinitionId, isRequired: false, OwnerUserId, UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            category.AddAttributeConfiguration(attributeDefinitionId, isRequired: true, OwnerUserId, UtcNow.AddMinutes(1)));
    }

    [Fact]
    public void SetAttributeRequired_UpdatesIsRequiredAndAuditFields()
    {
        var category = Category.Create(OwnerUserId, "Laptops", null, OwnerUserId, UtcNow);
        var attributeDefinitionId = Guid.NewGuid();
        category.AddAttributeConfiguration(attributeDefinitionId, isRequired: false, OwnerUserId, UtcNow);
        var updatedAt = UtcNow.AddMinutes(2);

        category.SetAttributeRequired(attributeDefinitionId, isRequired: true, "editor-1", updatedAt);

        var configuration = Assert.Single(category.AttributeConfigurations);
        Assert.True(configuration.IsRequired);
        Assert.Equal(updatedAt, category.UpdatedAtUtc);
        Assert.Equal("editor-1", category.UpdatedByUserId);
    }

    [Fact]
    public void SetAttributeRequired_NotConfigured_Throws()
    {
        var category = Category.Create(OwnerUserId, "Laptops", null, OwnerUserId, UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            category.SetAttributeRequired(Guid.NewGuid(), isRequired: true, OwnerUserId, UtcNow));
    }

    [Fact]
    public void RemoveAttributeConfiguration_RemovesAssociationAndUpdatesAuditFields()
    {
        var category = Category.Create(OwnerUserId, "Laptops", null, OwnerUserId, UtcNow);
        var attributeDefinitionId = Guid.NewGuid();
        category.AddAttributeConfiguration(attributeDefinitionId, isRequired: false, OwnerUserId, UtcNow);
        var updatedAt = UtcNow.AddMinutes(3);

        category.RemoveAttributeConfiguration(attributeDefinitionId, "editor-1", updatedAt);

        Assert.Empty(category.AttributeConfigurations);
        Assert.Equal(updatedAt, category.UpdatedAtUtc);
        Assert.Equal("editor-1", category.UpdatedByUserId);
    }

    [Fact]
    public void RemoveAttributeConfiguration_NotConfigured_Throws()
    {
        var category = Category.Create(OwnerUserId, "Laptops", null, OwnerUserId, UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            category.RemoveAttributeConfiguration(Guid.NewGuid(), OwnerUserId, UtcNow));
    }
}
