using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Application.Events.Dtos;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Events.Queries.GetEvents;

public sealed class GetEventsQueryHandler
    : IRequestHandler<GetEventsQuery, IReadOnlyList<EventDto>>
{
    private readonly IEventRepository _events;
    private readonly IEventTypeRepository _eventTypes;
    private readonly IEventStatusRepository _eventStatuses;
    private readonly IVenueRepository _venues;

    public GetEventsQueryHandler(
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

    public async Task<IReadOnlyList<EventDto>> Handle(
        GetEventsQuery request,
        CancellationToken cancellationToken)
    {
        // Los filtros por tipo y estado se traducen al id del catálogo (resuelto por nombre).
        // Si el valor pedido no existe en el catálogo, no hay coincidencias.
        Guid? eventTypeId = null;
        if (request.Type is { } type)
        {
            var lookup = await _eventTypes.GetByNameAsync(type.ToString(), cancellationToken);
            if (lookup is null)
            {
                return [];
            }

            eventTypeId = lookup.Id;
        }

        Guid? eventStatusId = null;
        if (request.Status is { } status)
        {
            var lookup = await _eventStatuses.GetByNameAsync(status.ToString(), cancellationToken);
            if (lookup is null)
            {
                return [];
            }

            eventStatusId = lookup.Id;
        }

        var events = await _events.GetAllAsync(cancellationToken);

        // Nombres de los catálogos para enriquecer la respuesta (id + nombre).
        var typeNames = (await _eventTypes.GetAllAsync(cancellationToken))
            .ToDictionary(x => x.Id, x => x.Name);
        var statusNames = (await _eventStatuses.GetAllAsync(cancellationToken))
            .ToDictionary(x => x.Id, x => x.Name);
        var venueNames = (await _venues.GetAllAsync(cancellationToken))
            .ToDictionary(x => x.Id, x => x.VenueName);

        return events
            .Where(e => eventTypeId is null || e.EventTypeId == eventTypeId)
            .Where(e => eventStatusId is null || e.EventStatusId == eventStatusId)
            .Where(e => request.VenueId is null || e.VenueId == request.VenueId)
            .Where(e => request.StartDateFrom is null || e.StartDate >= request.StartDateFrom)
            .Where(e => request.StartDateTo is null || e.StartDate <= request.StartDateTo)
            .Where(e => string.IsNullOrWhiteSpace(request.Title)
                || e.Title.Contains(request.Title.Trim(), StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.StartDate)
            .Select(e => EventDto.FromEntity(
                e,
                typeNames.GetValueOrDefault(e.EventTypeId, string.Empty),
                statusNames.GetValueOrDefault(e.EventStatusId, string.Empty),
                venueNames.GetValueOrDefault(e.VenueId, string.Empty)))
            .ToList();
    }
}
