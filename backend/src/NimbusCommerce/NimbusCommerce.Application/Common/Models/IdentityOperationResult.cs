namespace NimbusCommerce.Application.Common.Models;

/// <summary>
/// Outcome of an identity operation, decoupled from Microsoft.AspNetCore.Identity.IdentityResult
/// so Application does not depend on ASP.NET Core Identity.
/// </summary>
public sealed record IdentityOperationResult(bool Succeeded, IReadOnlyList<string> Errors)
{
    public static IdentityOperationResult Success() => new(true, Array.Empty<string>());

    public static IdentityOperationResult Failure(IEnumerable<string> errors) => new(false, errors.ToList());
}
