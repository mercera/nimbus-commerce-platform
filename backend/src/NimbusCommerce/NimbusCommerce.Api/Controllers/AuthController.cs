using Microsoft.AspNetCore.Mvc;
using NimbusCommerce.Api.Contracts.Authentication;
using NimbusCommerce.Application.Authentication.Login;
using NimbusCommerce.Application.Authentication.Refresh;
using NimbusCommerce.Application.Authentication.Register;

namespace NimbusCommerce.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private const string RefreshTokenCookieName = "refreshToken";
    private const string RefreshTokenCookiePath = "/api/auth/refresh";
    private const int MaxDeviceNameLength = 256;

    private readonly IRegisterService _registerService;
    private readonly ILoginService _loginService;
    private readonly IRefreshService _refreshService;

    public AuthController(IRegisterService registerService, ILoginService loginService, IRefreshService refreshService)
    {
        _registerService = registerService;
        _loginService = loginService;
        _refreshService = refreshService;
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
