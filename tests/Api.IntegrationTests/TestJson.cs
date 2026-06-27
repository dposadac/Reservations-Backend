using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ceiba.LiveEvent.Reservations.Api.IntegrationTests;

/// <summary>
/// Opciones de serialización para las pruebas: valores web (camelCase, case-insensitive)
/// más <see cref="JsonStringEnumConverter"/>, igual que la API, para poder leer los enums
/// que viajan como texto en las respuestas.
/// </summary>
internal static class TestJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
