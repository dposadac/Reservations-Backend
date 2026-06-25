using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace Ceiba.LiveEvent.Reservations.Api.OpenApi;

/// <summary>
/// Rellena la sección <c>info</c> del documento OpenAPI (título, versión, descripción,
/// contacto y licencia) a partir de <see cref="ApiDocumentationOptions"/>.
/// </summary>
public sealed class DocumentInfoTransformer : IOpenApiDocumentTransformer
{
    private readonly ApiDocumentationOptions _options;

    public DocumentInfoTransformer(IOptions<ApiDocumentationOptions> options)
        => _options = options.Value;

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info ??= new OpenApiInfo();
        document.Info.Title = _options.Title;
        document.Info.Version = _options.Version;
        document.Info.Description = _options.Description;

        if (!string.IsNullOrWhiteSpace(_options.ContactName)
            || !string.IsNullOrWhiteSpace(_options.ContactEmail)
            || !string.IsNullOrWhiteSpace(_options.ContactUrl))
        {
            document.Info.Contact = new OpenApiContact
            {
                Name = _options.ContactName,
                Email = _options.ContactEmail,
                Url = TryCreateUri(_options.ContactUrl)
            };
        }

        if (!string.IsNullOrWhiteSpace(_options.LicenseName))
        {
            document.Info.License = new OpenApiLicense
            {
                Name = _options.LicenseName!,
                Url = TryCreateUri(_options.LicenseUrl)
            };
        }

        return Task.CompletedTask;
    }

    private static Uri? TryCreateUri(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri) ? uri : null;
}
