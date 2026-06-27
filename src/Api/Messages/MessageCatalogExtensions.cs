using System.Text.Json;

namespace Ceiba.LiveEvent.Reservations.Api.Messages;

/// <summary>
/// Carga el catálogo de mensajes desde <c>Resources/messages.json</c> y registra
/// <see cref="IMessageService"/>. El JSON puede anidarse: las claves se aplanan con
/// puntos (<c>{ "error": { "notFound": ... } }</c> → <c>"error.notFound"</c>).
/// </summary>
public static class MessageCatalogExtensions
{
    public static IServiceCollection AddMessageCatalog(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        var path = Path.Combine(environment.ContentRootPath, "Resources", "messages.json");
        var messages = Load(path);

        services.AddSingleton<IMessageService>(new MessageService(messages));
        return services;
    }

    private static IReadOnlyDictionary<string, string> Load(string path)
    {
        var messages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!File.Exists(path))
        {
            return messages;
        }

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        Flatten(document.RootElement, prefix: string.Empty, messages);
        return messages;
    }

    private static void Flatten(JsonElement element, string prefix, IDictionary<string, string> target)
    {
        foreach (var property in element.EnumerateObject())
        {
            var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";

            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                Flatten(property.Value, key, target);
            }
            else
            {
                target[key] = property.Value.ToString();
            }
        }
    }
}
