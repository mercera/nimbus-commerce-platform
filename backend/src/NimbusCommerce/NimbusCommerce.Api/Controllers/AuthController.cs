using Microsoft.AspNetCore.Mvc;
using NimbusCommerce.Application.Authentication.Login;
using NimbusCommerce.Application.Authentication.Register;

namespace NimbusCommerce.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private const string RefreshTokenCookieName = "refreshToken";
    private const int MaxDeviceNameLength = 256;

    private readonly IRegisterService _registerService;
    private readonly ILoginService _loginService;

    public AuthController(IRegisterService registerService, ILoginService loginService)
    {
        _registerService = registerService;
        _loginService = loginService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _registerService.RegisterAsync(request);

        return result.Succeeded ? Ok() : BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _loginService.LoginAsync(request, GetDeviceName());

        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        Response.Cookies.Append(RefreshTokenCookieName, result.RefreshToken!, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = result.RefreshTokenExpiresAtUtc,
            Path = "/api/auth/refresh"
        });

        return Ok(new { accessToken = result.AccessToken, expiresAtUtc = result.AccessTokenExpiresAtUtc });
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
