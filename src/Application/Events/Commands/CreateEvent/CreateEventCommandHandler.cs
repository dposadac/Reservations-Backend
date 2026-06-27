using Ceiba.LiveEvent.Reservations.Application.Common.Exceptions;
using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Domain.Events;
using Ceiba.LiveEvent.Reservations.Domain.Venues;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Events.Commands.CreateEvent;

public sealed class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Guid>
{
    private readonly IEventRepository _events;
    private readonly IVenueRepository _venues;
    private readonly IEventTypeRepository _eventTypes;
    private readonly IEventStatusRepository _eventStatuses;
    private readonly IDateTimeProvider _clock;

    public CreateEventCommandHandler(
        IEventRepository events,
        IVenueRepository venues,
        IEventTypeRepository eventTypes,
        IEventStatusRepository eventStatuses,
        IDateTimeProvider clock)
    {
        _events = events;
        _venues = venues;
        _eventTypes = eventTypes;
        _eventStatuses = eventStatuses;
        _clock = clock;
    }

    public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        // RF-01: el venue debe existir; su capacidad acota la capacidad máxima del evento.
        var venue = await _venues.GetByIdAsync(request.VenueId, cancellationToken)
            ?? throw new NotFoundException(nameof(Venue), request.VenueId);

        // Resuelve el tipo de evento filtrando el catálogo por su nombre (texto), no por el
        // valor del enum; el id resultante (FK) es lo que persiste el evento.
        var typeName = request.Type.ToString();
        var eventType = await _eventTypes.GetByNameAsync(typeName, cancellationToken)
            ?? throw new NotFoundException(nameof(EventType), typeName);

        // Estado inicial por defecto: "activo" (resuelto contra el catálogo event_status).
        var activeName = EventStatus.Activo.ToString();
        var activeStatus = await _eventStatuses.GetByNameAsync(activeName, cancellationToken)
            ?? throw new NotFoundException(nameof(EventStatus), activeName);

        // RN-01/RN-03 se validan dentro de Event.Create.
        var @event = Event.Create(
            request.Title,
            request.Description,
            venue.Id,
            venue.Quantity,
            request.MaximumCapacity,
            request.StartDate,
            request.EndDate,
            request.TicketPrice,
            eventType.Id,
            activeStatus.Id,
            _clock.UtcNow);

        // RN-02: ningún otro evento activo del mismo venue puede solaparse en horario.
        var venueEvents = await _events.GetByVenueAsync(venue.Id, cancellationToken);
        var activeVenueEvents = venueEvents.Where(e => e.EventStatusId == activeStatus.Id);
        EventScheduling.EnsureNoVenueOverlap(@event.StartDate, @event.EndDate, activeVenueEvents);

        await _events.AddAsync(@event, cancellationToken);
        await _events.SaveChangesAsync(cancellationToken);

        return @event.Id;
    }
}
