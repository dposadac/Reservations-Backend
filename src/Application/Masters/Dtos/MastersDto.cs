using System.Text.Json.Serialization;

namespace Ceiba.LiveEvent.Reservations.Application.Masters.Dtos;

/// <summary>Ítem genérico de catálogo (id + nombre).</summary>
public sealed record MasterItemDto(Guid Id, string Name);

/// <summary>Proyección de un lugar para el listado de maestros.</summary>
public sealed record VenueMasterDto(Guid Id, string Name, int Capacity, string? City);

/// <summary>
/// Respuesta única con todas las tablas maestras del sistema.
/// </summary>
public sealed record MastersDto
{
    [JsonPropertyName("tipoEventos")]
    public IReadOnlyList<MasterItemDto> TipoEventos { get; init; } = [];

    [JsonPropertyName("eventosEstados")]
    public IReadOnlyList<MasterItemDto> EventosEstados { get; init; } = [];

    [JsonPropertyName("reservasestados")]
    public IReadOnlyList<MasterItemDto> ReservasEstados { get; init; } = [];

    [JsonPropertyName("venues")]
    public IReadOnlyList<VenueMasterDto> Venues { get; init; } = [];
}
