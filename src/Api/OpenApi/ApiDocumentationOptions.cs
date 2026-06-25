namespace Ceiba.LiveEvent.Reservations.Api.OpenApi;

/// <summary>
/// Opciones de la documentación OpenAPI/Swagger, configurables desde
/// <c>appsettings.json</c> (sección <see cref="SectionName"/>). Centralizarlas aquí
/// mantiene la configuración desacoplada del código y fácil de escalar.
/// </summary>
public sealed class ApiDocumentationOptions
{
    public const string SectionName = "ApiDocumentation";

    /// <summary>Nombre del documento OpenAPI y versión mostrada (p. ej. "v1").</summary>
    public string Version { get; set; } = "v1";

    public string Title { get; set; } = "Ceiba LiveEvent Reservations API";

    public string Description { get; set; } =
        "API REST para la gestión de eventos y reservas. " +
        "Construida con Clean Architecture, CQRS (MediatR) y FluentValidation.";

    public string? ContactName { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactUrl { get; set; }

    public string? LicenseName { get; set; }

    public string? LicenseUrl { get; set; }

    /// <summary>Si se expone el esquema de seguridad Bearer (JWT) en el documento.</summary>
    public bool EnableBearerSecurity { get; set; } = true;

    /// <summary>Ruta donde se sirve la UI de Swagger (sin barra inicial).</summary>
    public string RoutePrefix { get; set; } = "swagger";

    /// <summary>Permite exponer la documentación también fuera de Development.</summary>
    public bool ExposeInProduction { get; set; }
}
