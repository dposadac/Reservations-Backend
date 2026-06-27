using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Application.Masters.Dtos;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Masters.Queries.GetMasters;

public sealed class GetMastersQueryHandler : IRequestHandler<GetMastersQuery, MastersDto>
{
    private readonly IEventTypeRepository _eventTypes;
    private readonly IEventStatusRepository _eventStatuses;
    private readonly IReservationStatusRepository _reservationStatuses;
    private readonly IVenueRepository _venues;

    public GetMastersQueryHandler(
        IEventTypeRepository eventTypes,
        IEventStatusRepository eventStatuses,
        IReservationStatusRepository reservationStatuses,
        IVenueRepository venues)
    {
        _eventTypes = eventTypes;
        _eventStatuses = eventStatuses;
        _reservationStatuses = reservationStatuses;
        _venues = venues;
    }

    public async Task<MastersDto> Handle(GetMastersQuery request, CancellationToken cancellationToken)
    {
        var eventTypes = await _eventTypes.GetAllAsync(cancellationToken);
        var eventStatuses = await _eventStatuses.GetAllAsync(cancellationToken);
        var reservationStatuses = await _reservationStatuses.GetAllAsync(cancellationToken);
        var venues = await _venues.GetAllAsync(cancellationToken);

        return new MastersDto
        {
            TipoEventos = eventTypes.Select(x => new MasterItemDto(x.Id, x.Name)).ToList(),
            EventosEstados = eventStatuses.Select(x => new MasterItemDto(x.Id, x.Name)).ToList(),
            ReservasEstados = reservationStatuses.Select(x => new MasterItemDto(x.Id, x.Name)).ToList(),
            Venues = venues.Select(x => new VenueMasterDto(x.Id, x.VenueName, x.Quantity, x.City)).ToList()
        };
    }
}
