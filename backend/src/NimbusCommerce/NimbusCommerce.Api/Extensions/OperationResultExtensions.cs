using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Api.Extensions;

/// <summary>
/// Maps Application's transport-agnostic OperationResult/OperationResult&lt;T&gt; to HTTP
/// responses, so controllers stay thin and every catalogue failure produces one of exactly two
/// documented shapes: ValidationProblemDetails for ErrorCode.Validation, ProblemDetails
/// otherwise. Conflict and RuleViolation both map to 409 (they are both state conflicts); the
/// finer distinction is carried in the response's "errorCode" extension member rather than by
/// teaching the client a fifth HTTP status.
/// </summary>
public static class OperationResultExtensions
{
    public static IActionResult ToActionResult(this OperationResult result, ControllerBase controller) =>
        result.Succeeded
            ? controller.NoContent()
            : controller.ToProblemResult(result.Code, result.Message, result.Failures);

    public static IActionResult ToActionResult<T>(this OperationResult<T> result, ControllerBase controller) =>
        result.Succeeded
            ? controller.Ok(result.Value)
            : controller.ToProblemResult(result.Code, result.Message, result.Failures);

    public static IActionResult ToProblemResult(
        this ControllerBase controller, ErrorCode? errorCode, string? message, IReadOnlyList<ValidationFailure> failures)
    {
        if (errorCode == ErrorCode.Validation)
        {
            var modelState = new ModelStateDictionary();
            foreach (var failure in failures)
            {
                modelState.AddModelError(failure.Field, failure.Message);
            }

            return controller.ValidationProblem(modelState);
        }

        var statusCode = errorCode switch
        {
            ErrorCode.NotFound => StatusCodes.Status404NotFound,
            ErrorCode.Conflict => StatusCodes.Status409Conflict,
            ErrorCode.RuleViolation => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = errorCode?.ToString() ?? "Error",
            Detail = message
        };
        problemDetails.Extensions["errorCode"] = errorCode?.ToString();

        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }
}
