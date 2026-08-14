namespace NimbusCommerce.Application.Common.Models;

/// <summary>Per-field detail carried by an OperationResult.Invalid failure.</summary>
public sealed record ValidationFailure(string Field, string Message);
