using Ceiba.LiveEvent.Reservations.Application.Common.Exceptions;
using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Application.Events.Dtos;
using Ceiba.LiveEvent.Reservations.Domain.Events;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Events.Queries.GetEventById;

public sealed class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, EventDto>
{
    private readonly IEventRepository _events;
    private readonly IEventTypeRepository _eventTypes;
    private readonly IEventStatusRepository _eventStatuses;
    private readonly IVenueRepository _venues;

    public GetEventByIdQueryHandler(
        IEventRepository events,
        IEventTypeRepository eventTypes,
        IEventStatusRepository eventStatuses,
        IVenueRepository venues)
    {
        _events = events;
        _eventTypes = eventTypes;
        _eventStatuses = eventStatuses;
        _venues = venues;
    }

    public async Task<EventDto> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var @event = await _events.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Event), request.Id);

        // Resuelve los nombres de los catálogos para devolver id + nombre.
        var typeNames = (await _eventTypes.GetAllAsync(cancellationToken))
            .ToDictionary(x => x.Id, x => x.Name);
        var statusNames = (await _eventStatuses.GetAllAsync(cancellationToken))
            .ToDictionary(x => x.Id, x => x.Name);
        var venue = await _venues.GetByIdAsync(@event.VenueId, cancellationToken);

        return EventDto.FromEntity(
            @event,
            typeNames.GetValueOrDefault(@event.EventTypeId, string.Empty),
            statusNames.GetValueOrDefault(@event.EventStatusId, string.Empty),
            venue?.VenueName ?? string.Empty);
    }
}
