using Microsoft.Extensions.DependencyInjection;
using NimbusCommerce.Application.Authentication.CurrentUser;
using NimbusCommerce.Application.Authentication.Login;
using NimbusCommerce.Application.Authentication.Logout;
using NimbusCommerce.Application.Authentication.Refresh;
using NimbusCommerce.Application.Authentication.Register;
using NimbusCommerce.Application.Catalog.Categories.CreateCategory;
using NimbusCommerce.Application.Catalog.Categories.DeleteCategory;
using NimbusCommerce.Application.Catalog.Categories.GetCategory;
using NimbusCommerce.Application.Catalog.Categories.ListCategories;
using NimbusCommerce.Application.Catalog.Categories.SetCategoryStatus;
using NimbusCommerce.Application.Catalog.Categories.UpdateCategory;

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

        services.AddScoped<IListCategoriesService, ListCategoriesService>();
        services.AddScoped<IGetCategoryService, GetCategoryService>();
        services.AddScoped<ICreateCategoryService, CreateCategoryService>();
        services.AddScoped<IUpdateCategoryService, UpdateCategoryService>();
        services.AddScoped<ISetCategoryStatusService, SetCategoryStatusService>();
        services.AddScoped<IDeleteCategoryService, DeleteCategoryService>();

        return services;
    }
}
