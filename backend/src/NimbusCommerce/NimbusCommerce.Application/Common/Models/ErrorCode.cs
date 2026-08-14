namespace NimbusCommerce.Application.Common.Models;

/// <summary>
/// Failure classification for OperationResult. The Api layer maps each code to an HTTP status;
/// Application itself never references HTTP.
/// </summary>
public enum ErrorCode
{
    NotFound = 1,
    Conflict = 2,
    Validation = 3,
    RuleViolation = 4
}
