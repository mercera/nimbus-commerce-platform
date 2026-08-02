using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Authentication.Interfaces;

/// <summary>
/// Abstraction over the underlying identity provider. Application code must depend on this
/// interface instead of referencing ASP.NET Core Identity types directly.
/// </summary>
public interface IIdentityService
{
    Task<IdentityOperationResult> CreateUserAsync(string email, string password, string firstName, string lastName);

    Task<bool> CheckPasswordAsync(string userId, string password);

    Task<string?> GetUserIdAsync(string email);

    Task<IReadOnlyList<string>> GetRolesAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);
}
