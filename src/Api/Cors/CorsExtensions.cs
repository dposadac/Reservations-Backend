namespace Ceiba.LiveEvent.Reservations.Api.Cors;

/// <summary>
/// Configuración de CORS para permitir peticiones desde clientes web (SPA, etc.).
/// Mantiene el registro y el middleware fuera de <c>Program.cs</c>, igual que la
/// documentación OpenAPI.
/// </summary>
public static class CorsExtensions
{
    /// <summary>Registra la política de CORS a partir de la sección <c>Cors</c>.</summary>
    public static IServiceCollection AddApiCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()
            ?? new CorsOptions();

        services.AddCors(cors => cors.AddPolicy(CorsOptions.PolicyName, policy =>
        {
            policy.AllowAnyHeader().AllowAnyMethod();

            if (options.AllowedOrigins.Length > 0)
            {
                policy.WithOrigins(options.AllowedOrigins);

                // AllowCredentials es incompatible con "cualquier origen"; solo aquí.
                if (options.AllowCredentials)
                {
                    policy.AllowCredentials();
                }
            }
            else
            {
                // Sin orígenes configurados: API pública, cualquier origen sin credenciales.
                policy.AllowAnyOrigin();
            }
        }));

        return services;
    }

    /// <summary>Aplica el middleware de CORS con la política por defecto.</summary>
    public static WebApplication UseApiCors(this WebApplication app)
    {
        app.UseCors(CorsOptions.PolicyName);
        return app;
    }
}
