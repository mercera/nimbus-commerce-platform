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

    /// <summary>
    /// Returns the details needed to mint an access token for an already-authenticated subject,
    /// or null if the user no longer exists or has been deactivated. Not a credential check — the
    /// caller must have established identity by other means (e.g. a valid refresh token).
    /// </summary>
    Task<ActiveUser?> GetActiveUserAsync(string userId);
}

/// <summary>
/// A user who is still permitted to hold an access token, with the claims needed to issue one.
/// </summary>
public sealed record ActiveUser(string UserId, string Email, IReadOnlyList<string> Roles);
