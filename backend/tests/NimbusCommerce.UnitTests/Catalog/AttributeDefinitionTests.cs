using NimbusCommerce.Domain.Catalog;

namespace NimbusCommerce.UnitTests.Catalog;

// Covers only pure AttributeDefinition entity behavior. The "cannot delete a definition
// associated with any category" rule requires a database query and is enforced by
// DeleteAttributeDefinitionService, not AttributeDefinition itself, so it is not testable here —
// same boundary CategoryTests already documents for Category.
public class AttributeDefinitionTests
{
    private static readonly DateTime UtcNow = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);
    private const string OwnerUserId = "owner-1";

    [Fact]
    public void Create_SetsDefaultsAndAuditFields()
    {
        var attributeDefinition = AttributeDefinition.Create(OwnerUserId, "  Color  ", AttributeDataType.Text, OwnerUserId, UtcNow);

        Assert.NotEqual(Guid.Empty, attributeDefinition.Id);
        Assert.Equal(OwnerUserId, attributeDefinition.OwnerUserId);
        Assert.Equal("Color", attributeDefinition.Name);
        Assert.Equal(AttributeDataType.Text, attributeDefinition.DataType);
        Assert.True(attributeDefinition.IsActive);
        Assert.Equal(UtcNow, attributeDefinition.CreatedAtUtc);
        Assert.Equal(OwnerUserId, attributeDefinition.CreatedByUserId);
        Assert.Null(attributeDefinition.UpdatedAtUtc);
        Assert.Null(attributeDefinition.UpdatedByUserId);
        Assert.Empty(attributeDefinition.CategoryAssociations);
        Assert.Empty(attributeDefinition.ProductValues);
    }

    [Fact]
    public void Create_TrimsName()
    {
        var attributeDefinition = AttributeDefinition.Create(OwnerUserId, "  Weight  ", AttributeDataType.Number, OwnerUserId, UtcNow);

        Assert.Equal("Weight", attributeDefinition.Name);
    }

    [Fact]
    public void Rename_TrimsAndUpdatesNameAndAuditFields_LeavesDataTypeUnchanged()
    {
        var attributeDefinition = AttributeDefinition.Create(OwnerUserId, "Color", AttributeDataType.Text, OwnerUserId, UtcNow);
        var updatedAt = UtcNow.AddDays(1);

        attributeDefinition.Rename("  Primary Color  ", "editor-1", updatedAt);

        Assert.Equal("Primary Color", attributeDefinition.Name);
        Assert.Equal(AttributeDataType.Text, attributeDefinition.DataType);
        Assert.Equal(updatedAt, attributeDefinition.UpdatedAtUtc);
        Assert.Equal("editor-1", attributeDefinition.UpdatedByUserId);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalseAndUpdatesAuditFields()
    {
        var attributeDefinition = AttributeDefinition.Create(OwnerUserId, "Color", AttributeDataType.Text, OwnerUserId, UtcNow);
        var updatedAt = UtcNow.AddMinutes(1);

        attributeDefinition.Deactivate("editor-1", updatedAt);

        Assert.False(attributeDefinition.IsActive);
        Assert.Equal(updatedAt, attributeDefinition.UpdatedAtUtc);
        Assert.Equal("editor-1", attributeDefinition.UpdatedByUserId);
    }

    [Fact]
    public void Activate_SetsIsActiveTrueAndUpdatesAuditFields()
    {
        var attributeDefinition = AttributeDefinition.Create(OwnerUserId, "Color", AttributeDataType.Text, OwnerUserId, UtcNow);
        attributeDefinition.Deactivate(OwnerUserId, UtcNow.AddMinutes(1));
        var updatedAt = UtcNow.AddMinutes(2);

        attributeDefinition.Activate("editor-1", updatedAt);

        Assert.True(attributeDefinition.IsActive);
        Assert.Equal(updatedAt, attributeDefinition.UpdatedAtUtc);
        Assert.Equal("editor-1", attributeDefinition.UpdatedByUserId);
    }
}
