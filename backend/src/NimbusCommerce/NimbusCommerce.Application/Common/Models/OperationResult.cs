namespace NimbusCommerce.Application.Common.Models;

/// <summary>
/// Outcome of a catalogue use case that returns no data. Application never references HTTP —
/// ErrorCode is a meaningful, transport-agnostic classification; the Api layer maps it to a
/// status code and a ProblemDetails body (see Api/Extensions/OperationResultExtensions.cs).
///
/// This is an intentional evolution of the existing per-use-case Success/Failure record pattern
/// (IdentityOperationResult, LoginResult, RefreshResult), introduced because the catalogue has
/// ~25 use cases and four recurring failure shapes. The Authentication use cases are deliberately
/// not refactored onto this type as part of this milestone — see coding-standards.md.
/// </summary>
public sealed record OperationResult(bool Succeeded, ErrorCode? ErrorCode, string? Message, IReadOnlyList<ValidationFailure> Failures)
{
    public static OperationResult Success() => new(true, null, null, []);

    public static OperationResult NotFound(string message) => new(false, Models.ErrorCode.NotFound, message, []);

    public static OperationResult Conflict(string message) => new(false, Models.ErrorCode.Conflict, message, []);

    public static OperationResult RuleViolation(string message) => new(false, Models.ErrorCode.RuleViolation, message, []);

    public static OperationResult Invalid(IReadOnlyList<ValidationFailure> failures) =>
        new(false, Models.ErrorCode.Validation, "One or more validation errors occurred.", failures);
}

/// <summary>Same shape as OperationResult, carrying a value on success.</summary>
public sealed record OperationResult<T>(bool Succeeded, T? Value, ErrorCode? ErrorCode, string? Message, IReadOnlyList<ValidationFailure> Failures)
{
    public static OperationResult<T> Success(T value) => new(true, value, null, null, []);

    public static OperationResult<T> NotFound(string message) => new(false, default, Models.ErrorCode.NotFound, message, []);

    public static OperationResult<T> Conflict(string message) => new(false, default, Models.ErrorCode.Conflict, message, []);

    public static OperationResult<T> RuleViolation(string message) => new(false, default, Models.ErrorCode.RuleViolation, message, []);

    public static OperationResult<T> Invalid(IReadOnlyList<ValidationFailure> failures) =>
        new(false, default, Models.ErrorCode.Validation, "One or more validation errors occurred.", failures);
}
