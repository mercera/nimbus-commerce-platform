using Microsoft.Extensions.DependencyInjection;
using NimbusCommerce.Application.Authentication.CurrentUser;
using NimbusCommerce.Application.Authentication.Login;
using NimbusCommerce.Application.Authentication.Logout;
using NimbusCommerce.Application.Authentication.Refresh;
using NimbusCommerce.Application.Authentication.Register;
using NimbusCommerce.Application.Catalog.AttributeDefinitions.CreateAttributeDefinition;
using NimbusCommerce.Application.Catalog.AttributeDefinitions.DeleteAttributeDefinition;
using NimbusCommerce.Application.Catalog.AttributeDefinitions.GetAttributeDefinition;
using NimbusCommerce.Application.Catalog.AttributeDefinitions.ListAttributeDefinitionOptions;
using NimbusCommerce.Application.Catalog.AttributeDefinitions.ListAttributeDefinitions;
using NimbusCommerce.Application.Catalog.AttributeDefinitions.SetAttributeDefinitionStatus;
using NimbusCommerce.Application.Catalog.AttributeDefinitions.UpdateAttributeDefinition;
using NimbusCommerce.Application.Catalog.Categories.AttributeConfiguration.AddCategoryAttribute;
using NimbusCommerce.Application.Catalog.Categories.AttributeConfiguration.ListCategoryAttributes;
using NimbusCommerce.Application.Catalog.Categories.AttributeConfiguration.RemoveCategoryAttribute;
using NimbusCommerce.Application.Catalog.Categories.AttributeConfiguration.SetCategoryAttributeRequired;
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

        services.AddScoped<IListAttributeDefinitionsService, ListAttributeDefinitionsService>();
        services.AddScoped<IListAttributeDefinitionOptionsService, ListAttributeDefinitionOptionsService>();
        services.AddScoped<IGetAttributeDefinitionService, GetAttributeDefinitionService>();
        services.AddScoped<ICreateAttributeDefinitionService, CreateAttributeDefinitionService>();
        services.AddScoped<IUpdateAttributeDefinitionService, UpdateAttributeDefinitionService>();
        services.AddScoped<ISetAttributeDefinitionStatusService, SetAttributeDefinitionStatusService>();
        services.AddScoped<IDeleteAttributeDefinitionService, DeleteAttributeDefinitionService>();

        services.AddScoped<IListCategoryAttributesService, ListCategoryAttributesService>();
        services.AddScoped<IAddCategoryAttributeService, AddCategoryAttributeService>();
        services.AddScoped<ISetCategoryAttributeRequiredService, SetCategoryAttributeRequiredService>();
        services.AddScoped<IRemoveCategoryAttributeService, RemoveCategoryAttributeService>();

        return services;
    }
}
