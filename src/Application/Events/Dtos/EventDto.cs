using Ceiba.LiveEvent.Reservations.Domain.Events;

namespace Ceiba.LiveEvent.Reservations.Application.Events.Dtos;

/// <summary>Proyección de lectura de un evento para la capa de presentación.</summary>
public sealed record EventDto
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public Guid VenueId { get; init; }

    public string VenueName { get; init; } = string.Empty;

    public int MaximumCapacity { get; init; }

    public DateTimeOffset StartDate { get; init; }

    public DateTimeOffset EndDate { get; init; }

    public decimal TicketPrice { get; init; }

    public Guid EventTypeId { get; init; }

    public string EventTypeName { get; init; } = string.Empty;

    public Guid EventStatusId { get; init; }

    public string EventStatusName { get; init; } = string.Empty;

    public static EventDto FromEntity(
        Event @event,
        string eventTypeName = "",
        string eventStatusName = "",
        string venueName = "") => new()
    {
        Id = @event.Id,
        Title = @event.Title,
        Description = @event.Description,
        VenueId = @event.VenueId,
        VenueName = venueName,
        MaximumCapacity = @event.MaximumCapacity,
        StartDate = @event.StartDate,
        EndDate = @event.EndDate,
        TicketPrice = @event.TicketPrice,
        EventTypeId = @event.EventTypeId,
        EventTypeName = eventTypeName,
        EventStatusId = @event.EventStatusId,
        EventStatusName = eventStatusName
    };
}
