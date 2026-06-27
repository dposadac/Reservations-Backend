using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Application.Reservations.Dtos;
using Ceiba.LiveEvent.Reservations.Domain.Reservations;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Reservations.Queries.GetReservations;

public sealed class GetReservationsQueryHandler
    : IRequestHandler<GetReservationsQuery, IReadOnlyList<ReservationDto>>
{
    private readonly IReservationRepository _reservations;
    private readonly IReservationStatusRepository _reservationStatuses;

    public GetReservationsQueryHandler(
        IReservationRepository reservations,
        IReservationStatusRepository reservationStatuses)
    {
        _reservations = reservations;
        _reservationStatuses = reservationStatuses;
    }

    public async Task<IReadOnlyList<ReservationDto>> Handle(
        GetReservationsQuery request,
        CancellationToken cancellationToken)
    {
        // El filtro por estado se traduce al id del catálogo (resuelto por nombre).
        Guid? reservationStatusId = null;
        if (request.Status is { } status)
        {
            var lookup = await _reservationStatuses.GetByNameAsync(
                ReservationStatusNames.For(status), cancellationToken);
            if (lookup is null)
            {
                return [];
            }

            reservationStatusId = lookup.Id;
        }

        var reservations = request.EventId is { } eventId
            ? await _reservations.GetByEventAsync(eventId, cancellationToken)
            : await _reservations.GetAllAsync(cancellationToken);

        // Nombres del catálogo de estados para devolver id + nombre.
        var statusNames = (await _reservationStatuses.GetAllAsync(cancellationToken))
            .ToDictionary(x => x.Id, x => x.Name);

        return reservations
            .Where(r => reservationStatusId is null || r.ReservationStatusId == reservationStatusId)
            .Select(r => ReservationDto.FromEntity(
                r,
                statusNames.GetValueOrDefault(r.ReservationStatusId, string.Empty)))
            .ToList();
    }
}
