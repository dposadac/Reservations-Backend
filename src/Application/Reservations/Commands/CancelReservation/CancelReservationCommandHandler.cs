using Ceiba.LiveEvent.Reservations.Application.Common.Exceptions;
using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Domain.Events;
using Ceiba.LiveEvent.Reservations.Domain.Reservations;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Reservations.Commands.CancelReservation;

public sealed class CancelReservationCommandHandler : IRequestHandler<CancelReservationCommand, bool>
{
    private readonly IReservationRepository _reservations;
    private readonly IReservationStatusRepository _reservationStatuses;
    private readonly IEventRepository _events;
    private readonly IDateTimeProvider _clock;

    public CancelReservationCommandHandler(
        IReservationRepository reservations,
        IReservationStatusRepository reservationStatuses,
        IEventRepository events,
        IDateTimeProvider clock)
    {
        _reservations = reservations;
        _reservationStatuses = reservationStatuses;
        _events = events;
        _clock = clock;
    }

    public async Task<bool> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _reservations.GetByIdAsync(request.ReservationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), request.ReservationId);

        // RN-07 necesita la fecha de inicio del evento para decidir la penalización.
        var @event = await _events.GetByIdAsync(reservation.EventId, cancellationToken)
            ?? throw new NotFoundException(nameof(Event), reservation.EventId);

        // Ids de los estados implicados (catálogo): "cancelada" (destino) y "pendiente_pago" (guarda).
        var cancelledStatus = await _reservationStatuses.GetByNameAsync(
            ReservationStatusNames.Cancelled, cancellationToken)
            ?? throw new NotFoundException(nameof(ReservationStatus), ReservationStatusNames.Cancelled);
        var pendingStatus = await _reservationStatuses.GetByNameAsync(
            ReservationStatusNames.PendingPayment, cancellationToken)
            ?? throw new NotFoundException(nameof(ReservationStatus), ReservationStatusNames.PendingPayment);

        reservation.Cancel(_clock.UtcNow, cancelledStatus.Id, pendingStatus.Id, @event.StartDate);

        _reservations.Update(reservation);
        await _reservations.SaveChangesAsync(cancellationToken);

        return reservation.IsForfeited;
    }
}
