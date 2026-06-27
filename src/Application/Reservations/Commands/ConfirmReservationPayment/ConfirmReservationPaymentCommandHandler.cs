using Ceiba.LiveEvent.Reservations.Application.Common.Exceptions;
using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Domain.Reservations;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Reservations.Commands.ConfirmReservationPayment;

public sealed class ConfirmReservationPaymentCommandHandler
    : IRequestHandler<ConfirmReservationPaymentCommand, string>
{
    private readonly IReservationRepository _reservations;
    private readonly IReservationStatusRepository _reservationStatuses;

    public ConfirmReservationPaymentCommandHandler(
        IReservationRepository reservations,
        IReservationStatusRepository reservationStatuses)
    {
        _reservations = reservations;
        _reservationStatuses = reservationStatuses;
    }

    public async Task<string> Handle(
        ConfirmReservationPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var reservation = await _reservations.GetByIdAsync(request.ReservationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), request.ReservationId);

        // Ids de los estados implicados (catálogo): "confirmada" (destino) y "cancelada" (guarda).
        var confirmedStatus = await _reservationStatuses.GetByNameAsync(
            ReservationStatusNames.Confirmed, cancellationToken)
            ?? throw new NotFoundException(nameof(ReservationStatus), ReservationStatusNames.Confirmed);
        var cancelledStatus = await _reservationStatuses.GetByNameAsync(
            ReservationStatusNames.Cancelled, cancellationToken)
            ?? throw new NotFoundException(nameof(ReservationStatus), ReservationStatusNames.Cancelled);

        // RF-04: genera un código único EV-######. La unicidad final la respalda el
        // índice único de la BD; aquí reintentamos ante una colisión improbable.
        ReservationCode code;
        do
        {
            code = ReservationCode.New();
        }
        while (await _reservations.ExistsByCodeAsync(code.Value, cancellationToken));

        reservation.ConfirmPayment(code, confirmedStatus.Id, cancelledStatus.Id);

        _reservations.Update(reservation);
        await _reservations.SaveChangesAsync(cancellationToken);

        return code.Value;
    }
}
