using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Ceiba.LiveEvent.Reservations.Api.OpenApi;

/// <summary>
/// Declara el esquema de seguridad <c>Bearer</c> (JWT) en el documento OpenAPI, de modo
/// que la UI de Swagger muestre el botón "Authorize". El API aún no exige autenticación;
/// el esquema se documenta para dejar la base preparada y escalable.
/// </summary>
public sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    private const string SchemeId = "Bearer";

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes[SchemeId] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Autenticación JWT. Introduce únicamente el token (sin el prefijo 'Bearer')."
        };

        var requirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(SchemeId, document)] = new List<string>()
        };

        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(requirement);

        return Task.CompletedTask;
    }
}
