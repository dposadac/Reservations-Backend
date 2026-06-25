using Microsoft.Extensions.Options;

namespace Ceiba.LiveEvent.Reservations.Api.OpenApi;

/// <summary>
/// Punto único de configuración de la documentación OpenAPI/Swagger. Mantener el registro
/// y el middleware aquí deja <c>Program.cs</c> limpio y permite escalar (más documentos,
/// versiones o transformers) sin tocar el arranque de la aplicación.
/// </summary>
public static class OpenApiExtensions
{
    /// <summary>Registra la generación del documento OpenAPI y sus transformers.</summary>
    public static IServiceCollection AddApiDocumentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(ApiDocumentationOptions.SectionName);
        services.AddOptions<ApiDocumentationOptions>().Bind(section);

        var options = section.Get<ApiDocumentationOptions>() ?? new ApiDocumentationOptions();

        services.AddOpenApi(options.Version, openApi =>
        {
            openApi.AddDocumentTransformer<DocumentInfoTransformer>();

            if (options.EnableBearerSecurity)
            {
                openApi.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            }
        });

        return services;
    }

    /// <summary>Expone el JSON OpenAPI y la UI de Swagger según el entorno.</summary>
    public static WebApplication UseApiDocumentation(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<ApiDocumentationOptions>>().Value;

        if (!app.Environment.IsDevelopment() && !options.ExposeInProduction)
        {
            return app;
        }

        // Documento JSON en /openapi/{version}.json
        app.MapOpenApi();

        // UI de Swagger en /{RoutePrefix}
        app.UseSwaggerUI(ui =>
        {
            ui.SwaggerEndpoint($"/openapi/{options.Version}.json", $"{options.Title} {options.Version}");
            ui.RoutePrefix = options.RoutePrefix;
            ui.DocumentTitle = options.Title;
        });

        return app;
    }
}
