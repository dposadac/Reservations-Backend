using Ceiba.LiveEvent.Reservations.Domain.Events;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Events.Commands.CreateEvent;

/// <summary>Comando para crear un evento (RF-01). Devuelve el identificador generado.</summary>
public sealed record CreateEventCommand : IRequest<Guid>
{
    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public Guid VenueId { get; init; }

    public int MaximumCapacity { get; init; }

    public DateTimeOffset StartDate { get; init; }

    public DateTimeOffset EndDate { get; init; }

    public decimal TicketPrice { get; init; }

    public EventType Type { get; init; }
}
