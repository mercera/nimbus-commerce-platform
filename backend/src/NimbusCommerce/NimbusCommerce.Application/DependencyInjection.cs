using Microsoft.Extensions.DependencyInjection;
using NimbusCommerce.Application.Authentication.Login;
using NimbusCommerce.Application.Authentication.Register;

namespace NimbusCommerce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRegisterService, RegisterService>();
        services.AddScoped<ILoginService, LoginService>();

        return services;
    }
}
