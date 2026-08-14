using Microsoft.Extensions.DependencyInjection;
using NimbusCommerce.Application.Authentication.CurrentUser;
using NimbusCommerce.Application.Authentication.Login;
using NimbusCommerce.Application.Authentication.Logout;
using NimbusCommerce.Application.Authentication.Refresh;
using NimbusCommerce.Application.Authentication.Register;

namespace NimbusCommerce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRegisterService, RegisterService>();
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IRefreshService, RefreshService>();
        services.AddScoped<ILogoutService, LogoutService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
