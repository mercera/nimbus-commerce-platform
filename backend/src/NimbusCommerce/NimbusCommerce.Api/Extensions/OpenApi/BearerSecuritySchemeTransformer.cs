using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace NimbusCommerce.Api.Extensions.OpenApi;

/// <summary>
/// Declares the JWT bearer scheme on the generated OpenAPI document and applies it to
/// endpoints that require authorization. Without this, AddOpenApi() emits no security
/// scheme at all and an API explorer's "Authorize" control has nothing to bind to.
/// </summary>
internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider schemeProvider)
    : IOpenApiDocumentTransformer
{
    private const string SchemeId = "Bearer";

    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var schemes = await schemeProvider.GetAllSchemesAsync();
        if (!schemes.Any(scheme => scheme.Name == JwtBearerDefaults.AuthenticationScheme))
        {
            return;
        }

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SchemeId] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Paste the accessToken returned by POST /api/auth/login. The 'Bearer ' prefix is added automatically."
        };

        ApplyRequirementToSecuredOperations(document, context);
    }

    /// <summary>
    /// Marks only those operations whose endpoint carries an authorization requirement.
    /// Applying the requirement document-wide would make anonymous endpoints such as
    /// login and register appear to need a token.
    /// </summary>
    private static void ApplyRequirementToSecuredOperations(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context)
    {
        var securedOperationIds = context.DescriptionGroups
            .SelectMany(group => group.Items)
            .Where(description => RequiresAuthorization(description.ActionDescriptor.EndpointMetadata))
            .Select(description => description.ActionDescriptor.Id)
            .ToHashSet();

        if (securedOperationIds.Count == 0)
        {
            return;
        }

        foreach (var description in context.DescriptionGroups.SelectMany(group => group.Items))
        {
            if (!securedOperationIds.Contains(description.ActionDescriptor.Id))
            {
                continue;
            }

            var operation = FindOperation(document, description.RelativePath, description.HttpMethod);
            if (operation is null)
            {
                continue;
            }

            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(SchemeId, document)] = []
            });
        }
    }

    private static bool RequiresAuthorization(IList<object> endpointMetadata) =>
        !endpointMetadata.OfType<IAllowAnonymous>().Any()
        && endpointMetadata.OfType<IAuthorizeData>().Any();

    private static OpenApiOperation? FindOperation(OpenApiDocument document, string? relativePath, string? httpMethod)
    {
        if (relativePath is null || httpMethod is null || document.Paths is null)
        {
            return null;
        }

        if (!document.Paths.TryGetValue($"/{relativePath.TrimStart('/')}", out var pathItem)
            || pathItem.Operations is null)
        {
            return null;
        }

        var method = new HttpMethod(httpMethod);

        return pathItem.Operations.TryGetValue(method, out var operation) ? operation : null;
    }
}
