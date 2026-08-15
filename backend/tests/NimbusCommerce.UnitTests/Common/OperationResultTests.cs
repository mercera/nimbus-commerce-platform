using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.UnitTests.Common;

public class OperationResultTests
{
    [Fact]
    public void Success_SetsSucceededTrueWithNoCodeOrMessageOrFailures()
    {
        var result = OperationResult.Success();

        Assert.True(result.Succeeded);
        Assert.Null(result.Code);
        Assert.Null(result.Message);
        Assert.Empty(result.Failures);
    }

    [Fact]
    public void NotFound_SetsNotFoundCodeAndMessage()
    {
        var result = OperationResult.NotFound("Category not found.");

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCode.NotFound, result.Code);
        Assert.Equal("Category not found.", result.Message);
        Assert.Empty(result.Failures);
    }

    [Fact]
    public void Conflict_SetsConflictCodeAndMessage()
    {
        var result = OperationResult.Conflict("A category named 'Laptops' already exists.");

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCode.Conflict, result.Code);
        Assert.Equal("A category named 'Laptops' already exists.", result.Message);
        Assert.Empty(result.Failures);
    }

    [Fact]
    public void RuleViolation_SetsRuleViolationCodeAndMessage()
    {
        var result = OperationResult.RuleViolation("Cannot deactivate this category: it has 1 active product(s).");

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCode.RuleViolation, result.Code);
        Assert.Equal("Cannot deactivate this category: it has 1 active product(s).", result.Message);
        Assert.Empty(result.Failures);
    }

    [Fact]
    public void Invalid_SetsValidationCodeAndCarriesFailures()
    {
        var failures = new[] { new ValidationFailure("Name", "Name is required.") };

        var result = OperationResult.Invalid(failures);

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCode.Validation, result.Code);
        Assert.Equal("One or more validation errors occurred.", result.Message);
        Assert.Equal(failures, result.Failures);
    }
}

public class OperationResultOfTTests
{
    [Fact]
    public void Success_SetsSucceededTrueWithValueAndNoCodeOrMessageOrFailures()
    {
        var result = OperationResult<string>.Success("category-detail");

        Assert.True(result.Succeeded);
        Assert.Equal("category-detail", result.Value);
        Assert.Null(result.Code);
        Assert.Null(result.Message);
        Assert.Empty(result.Failures);
    }

    [Fact]
    public void NotFound_SetsNotFoundCodeAndDefaultValue()
    {
        var result = OperationResult<string>.NotFound("Category not found.");

        Assert.False(result.Succeeded);
        Assert.Null(result.Value);
        Assert.Equal(ErrorCode.NotFound, result.Code);
        Assert.Equal("Category not found.", result.Message);
        Assert.Empty(result.Failures);
    }

    [Fact]
    public void Conflict_SetsConflictCodeAndDefaultValue()
    {
        var result = OperationResult<string>.Conflict("A category named 'Laptops' already exists.");

        Assert.False(result.Succeeded);
        Assert.Null(result.Value);
        Assert.Equal(ErrorCode.Conflict, result.Code);
        Assert.Equal("A category named 'Laptops' already exists.", result.Message);
        Assert.Empty(result.Failures);
    }

    [Fact]
    public void RuleViolation_SetsRuleViolationCodeAndDefaultValue()
    {
        var result = OperationResult<string>.RuleViolation("Cannot delete this category: it has 1 product(s).");

        Assert.False(result.Succeeded);
        Assert.Null(result.Value);
        Assert.Equal(ErrorCode.RuleViolation, result.Code);
        Assert.Equal("Cannot delete this category: it has 1 product(s).", result.Message);
        Assert.Empty(result.Failures);
    }

    [Fact]
    public void Invalid_SetsValidationCodeAndCarriesFailures()
    {
        var failures = new[] { new ValidationFailure("Name", "Name is required.") };

        var result = OperationResult<string>.Invalid(failures);

        Assert.False(result.Succeeded);
        Assert.Null(result.Value);
        Assert.Equal(ErrorCode.Validation, result.Code);
        Assert.Equal("One or more validation errors occurred.", result.Message);
        Assert.Equal(failures, result.Failures);
    }
}
