namespace Ceiba.LiveEvent.Reservations.Api.Cors;

/// <summary>Opciones de CORS enlazadas desde la sección <c>Cors</c> de la configuración.</summary>
public sealed class CorsOptions
{
    public const string SectionName = "Cors";
    public const string PolicyName = "DefaultCorsPolicy";

    /// <summary>
    /// Orígenes permitidos (p. ej. <c>https://mi-frontend.com</c>). Si está vacío se
    /// permite cualquier origen (sin credenciales), útil para una API pública.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = [];

    /// <summary>
    /// Permite el envío de credenciales (cookies / cabecera Authorization). Solo aplica
    /// cuando se especifican orígenes concretos; no es compatible con "cualquier origen".
    /// </summary>
    public bool AllowCredentials { get; set; }
}
