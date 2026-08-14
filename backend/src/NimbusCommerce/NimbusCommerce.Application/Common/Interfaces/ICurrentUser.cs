namespace NimbusCommerce.Application.Common.Interfaces;

/// <summary>
/// Ambient accessor for the authenticated caller's user id, consumed by catalogue services so
/// userId does not need to be threaded through every method signature by hand.
///
/// Not to be confused with ICurrentUserService (Application/Authentication/CurrentUser) — that is
/// the GET /api/auth/me use case, which takes a userId parameter it did not look up itself. This
/// is the ambient "who is calling?" accessor that ICurrentUserService's own doc comment says does
/// not exist yet; introduced now that the catalogue gives it more than one consumer (see
/// Architecture.md, "Current user use case").
/// </summary>
public interface ICurrentUser
{
    string? UserId { get; }

    /// <summary>
    /// Returns UserId, or throws if null. Every catalogue endpoint is [Authorize], so a null
    /// subject here means a missing [Authorize] attribute, not a reachable runtime path.
    /// </summary>
    string RequireUserId();
}
