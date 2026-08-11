using NimbusCommerce.Application.Authentication.Interfaces;

namespace NimbusCommerce.Application.Authentication.CurrentUser;

internal sealed class CurrentUserService : ICurrentUserService
{
    private readonly IIdentityService _identityService;

    public CurrentUserService(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    /// <summary>
    /// Deliberately a single delegation. The lookup and the "still active" rule both belong to the
    /// identity provider, so there is nothing to orchestrate here — but routing the use case
    /// through Application keeps <c>AuthController</c> talking only to use cases, never directly to
    /// identity primitives, and gives this endpoint's deactivation policy a named owner to grow into.
    /// </summary>
    public Task<UserProfile?> GetCurrentUserAsync(string userId) =>
        _identityService.GetUserProfileAsync(userId);
}
