using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NimbusCommerce.Application.Common.Interfaces;

namespace NimbusCommerce.Infrastructure.Identity;

internal sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // The token's `sub` claim arrives as ClaimTypes.NameIdentifier, NOT as
    // JwtRegisteredClaimNames.Sub: JwtBearerOptions.MapInboundClaims defaults to true, and the
    // inbound map rewrites "sub" before the principal reaches here. Same trap AuthController.Me()
    // documents; see engineering-handbook.md — verified empirically, not inferred.
    public string? UserId =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public string RequireUserId() =>
        UserId ?? throw new InvalidOperationException(
            "No authenticated user is available on the current request. This indicates a missing [Authorize] attribute.");
}
