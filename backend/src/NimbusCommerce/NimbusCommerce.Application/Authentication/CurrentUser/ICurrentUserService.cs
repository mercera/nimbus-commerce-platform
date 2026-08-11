using NimbusCommerce.Application.Authentication.Interfaces;

namespace NimbusCommerce.Application.Authentication.CurrentUser;

/// <summary>
/// Retrieves the live account state of the authenticated caller.
///
/// Not to be confused with an ambient "who is calling?" accessor (an <c>ICurrentUser</c> reading
/// <c>HttpContext.User</c>), which does not exist in this solution. This is a use case: the caller
/// establishes identity itself and passes the resulting user id in.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Returns the caller's current profile, or null if the account no longer exists or has been
    /// deactivated since their access token was issued.
    /// </summary>
    Task<UserProfile?> GetCurrentUserAsync(string userId);
}
