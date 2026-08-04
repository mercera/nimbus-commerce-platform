using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Authentication.Interfaces;

/// <summary>
/// Abstraction over the underlying identity provider. Application code must depend on this
/// interface instead of referencing ASP.NET Core Identity types directly.
/// </summary>
public interface IIdentityService
{
    Task<IdentityOperationResult> CreateUserAsync(string email, string password, string firstName, string lastName);

    /// <summary>
    /// Validates credentials and returns the matching user's id, or null if the email is unknown,
    /// the password is wrong, or the account is inactive/locked out. Callers must treat all of these
    /// cases identically to avoid leaking account existence.
    /// </summary>
    Task<string?> ValidateCredentialsAsync(string email, string password);

    Task<IReadOnlyList<string>> GetRolesAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);
}
