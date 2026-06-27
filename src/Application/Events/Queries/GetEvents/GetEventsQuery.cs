using Ceiba.LiveEvent.Reservations.Application.Events.Dtos;
using Ceiba.LiveEvent.Reservations.Domain.Events;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Events.Queries.GetEvents;

/// <summary>Consulta para listar eventos con filtros opcionales (RF-02).</summary>
public sealed record GetEventsQuery : IRequest<IReadOnlyList<EventDto>>
{
    /// <summary>Filtra por tipo de evento.</summary>
    public EventType? Type { get; init; }

    /// <summary>Filtra por estado (activo, cancelado, completado).</summary>
    public EventStatus? Status { get; init; }

    /// <summary>Filtra por lugar.</summary>
    public Guid? VenueId { get; init; }

    /// <summary>Inicio del rango de fecha de inicio (inclusive).</summary>
    public DateTimeOffset? StartDateFrom { get; init; }

    /// <summary>Fin del rango de fecha de inicio (inclusive).</summary>
    public DateTimeOffset? StartDateTo { get; init; }

    /// <summary>Búsqueda parcial por título (case-insensitive).</summary>
    public string? Title { get; init; }
}
