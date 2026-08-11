using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NimbusCommerce.Api.Contracts.Authentication;
using NimbusCommerce.Application.Authentication.CurrentUser;
using NimbusCommerce.Application.Authentication.Login;
using NimbusCommerce.Application.Authentication.Logout;
using NimbusCommerce.Application.Authentication.Refresh;
using NimbusCommerce.Application.Authentication.Register;

namespace NimbusCommerce.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private const string RefreshTokenCookieName = "refreshToken";

    // Must cover every /api/auth/* endpoint that reads the cookie (Refresh, Logout, and any
    // future one) — do not narrow this back to a single endpoint's route.
    private const string RefreshTokenCookiePath = "/api/auth";
    private const int MaxDeviceNameLength = 256;

    private readonly IRegisterService _registerService;
    private readonly ILoginService _loginService;
    private readonly IRefreshService _refreshService;
    private readonly ILogoutService _logoutService;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(
        IRegisterService registerService,
        ILoginService loginService,
        IRefreshService refreshService,
        ILogoutService logoutService,
        ICurrentUserService currentUserService)
    {
        _registerService = registerService;
        _loginService = loginService;
        _refreshService = refreshService;
        _logoutService = logoutService;
        _currentUserService = currentUserService;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _registerService.RegisterAsync(request);

        return result.Succeeded ? Ok() : BadRequest(result.Errors);
    }

    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _loginService.LoginAsync(request, GetDeviceName());
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        SetRefreshTokenCookie(result.RefreshToken!, result.RefreshTokenExpiresAtUtc!.Value);

        return Ok(new LoginResponse(result.AccessToken!, result.AccessTokenExpiresAtUtc!.Value));
    }

    [HttpPost("refresh")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)] // same wire shape as login, deliberately reused
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh()
    {
        Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken);

        var result = await _refreshService.RefreshAsync(refreshToken ?? string.Empty, GetDeviceName());
        if (!result.Succeeded)
        {
            // The cookie is useless (and possibly stolen) whatever the cause; never leave it behind.
            DeleteRefreshTokenCookie();
            return Unauthorized(new { message = "Invalid or expired refresh token." });
        }

        SetRefreshTokenCookie(result.RefreshToken!, result.RefreshTokenExpiresAtUtc!.Value);

        return Ok(new LoginResponse(result.AccessToken!, result.AccessTokenExpiresAtUtc!.Value));
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        // No [Authorize]: the refresh-token cookie identifies the session being terminated,
        // and a user must be able to log out even after their access token has expired.
        //
        // If [Authorize] is ever added here, the token's owning UserId must be checked against
        // the authenticated user's `sub` claim before revoking — otherwise an authenticated
        // user could revoke another user's session by presenting that user's cookie. Safe today
        // only because the endpoint is anonymous and the cookie alone determines what is revoked.
        Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken);

        await _logoutService.LogoutAsync(refreshToken ?? string.Empty);

        // Always cleared, whether or not a stored token matched — logout must be idempotent.
        DeleteRefreshTokenCookie();

        return NoContent();
    }

    // [Authorize] sits on the action, not the controller: four of the five actions here are
    // deliberately anonymous (see the Logout comment above). Once protected actions outnumber
    // anonymous ones, move [Authorize] to the controller and mark the exceptions
    // [AllowAnonymous] instead — that default is fail-safe, this one is not.
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<MeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me()
    {
        // The token's `sub` claim arrives as ClaimTypes.NameIdentifier, NOT as
        // JwtRegisteredClaimNames.Sub: JwtBearerOptions.MapInboundClaims defaults to true, and the
        // inbound map rewrites "sub" before the principal reaches this action. Reading
        // JwtRegisteredClaimNames.Sub here returns null and every request 401s.
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // [Authorize] proves the token was signed by us and is unexpired; it does not prove the
        // token carries a subject. Only TokenService mints tokens, so this should be unreachable.
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Account is no longer active." });
        }

        // The id comes only from the validated token — never from a route or query parameter — so
        // a caller can only ever read their own record. Keep it that way.
        var user = await _currentUserService.GetCurrentUserAsync(userId);
        if (user is null)
        {
            // 401, not 403: the account itself is gone or deactivated, so the credential should no
            // longer be honoured anywhere and the client should re-authenticate. This matches how
            // RefreshService already treats a deactivated user, so the client's refresh retry will
            // fail too and land them at login.
            return Unauthorized(new { message = "Account is no longer active." });
        }

        return Ok(new MeResponse(user.UserId, user.Email, user.FirstName, user.LastName, user.Roles));
    }

    private void SetRefreshTokenCookie(string rawToken, DateTime expiresAtUtc)
    {
        Response.Cookies.Append(RefreshTokenCookieName, rawToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expiresAtUtc,
            Path = RefreshTokenCookiePath
        });
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = RefreshTokenCookiePath
        });
    }

    private string? GetDeviceName()
    {
        var userAgent = Request.Headers.UserAgent.ToString();
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return null;
        }

        return userAgent.Length > MaxDeviceNameLength ? userAgent[..MaxDeviceNameLength] : userAgent;
    }
}
