namespace NimbusCommerce.Application.Authentication.Logout;

public interface ILogoutService
{
    Task LogoutAsync(string refreshToken);
}
