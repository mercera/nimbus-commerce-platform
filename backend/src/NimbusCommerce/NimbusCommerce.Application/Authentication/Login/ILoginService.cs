namespace NimbusCommerce.Application.Authentication.Login;

public interface ILoginService
{
    Task<LoginResult> LoginAsync(LoginRequest request, string? deviceName);
}
