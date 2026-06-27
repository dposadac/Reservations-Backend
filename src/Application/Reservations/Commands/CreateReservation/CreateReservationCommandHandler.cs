using Ceiba.LiveEvent.Reservations.Application.Common.Exceptions;
using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Domain.Events;
using Ceiba.LiveEvent.Reservations.Domain.Exceptions;
using Ceiba.LiveEvent.Reservations.Domain.Reservations;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Reservations.Commands.CreateReservation;

public sealed class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, Guid>
{
    private readonly IReservationRepository _reservations;
    private readonly IEventRepository _events;
    private readonly IEventStatusRepository _eventStatuses;
    private readonly IReservationStatusRepository _reservationStatuses;
    private readonly IDateTimeProvider _clock;

    public CreateReservationCommandHandler(
        IReservationRepository reservations,
        IEventRepository events,
        IEventStatusRepository eventStatuses,
        IReservationStatusRepository reservationStatuses,
        IDateTimeProvider clock)
    {
        _reservations = reservations;
        _events = events;
        _eventStatuses = eventStatuses;
        _reservationStatuses = reservationStatuses;
        _clock = clock;
    }

    public async Task<Guid> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var @event = await _events.GetByIdAsync(request.EventId, cancellationToken)
            ?? throw new NotFoundException(nameof(Event), request.EventId);

        // El evento debe estar "activo"; se compara contra el id del catálogo.
        var activeName = EventStatus.Activo.ToString();
        var activeStatus = await _eventStatuses.GetByNameAsync(activeName, cancellationToken)
            ?? throw new NotFoundException(nameof(EventStatus), activeName);

        if (@event.EventStatusId != activeStatus.Id)
        {
            throw new DomainException("Solo se pueden reservar entradas de un evento activo.");
        }

        // Ids de los estados de reserva (catálogo): "pendiente_pago" (inicial) y "confirmada"
        // (para el cálculo de aforo).
        var pendingStatus = await _reservationStatuses.GetByNameAsync(
            ReservationStatusNames.PendingPayment, cancellationToken)
            ?? throw new NotFoundException(nameof(ReservationStatus), ReservationStatusNames.PendingPayment);
        var confirmedStatus = await _reservationStatuses.GetByNameAsync(
            ReservationStatusNames.Confirmed, cancellationToken)
            ?? throw new NotFoundException(nameof(ReservationStatus), ReservationStatusNames.Confirmed);

        // RF-03: la disponibilidad se calcula sobre las reservas vigentes del evento.
        var existing = await _reservations.GetByEventAsync(@event.Id, cancellationToken);
        var available = ReservationTicketing.AvailableForBooking(
            @event.MaximumCapacity, existing, pendingStatus.Id, confirmedStatus.Id);

        // RN-04/RN-05 se validan dentro de Reservation.Create.
        var reservation = Reservation.Create(
            @event.Id,
            request.Quantity,
            request.PurchaserName,
            request.PurchaserEmail,
            available,
            @event.StartDate,
            @event.TicketPrice,
            _clock.UtcNow,
            pendingStatus.Id,
            request.City);

        await _reservations.AddAsync(reservation, cancellationToken);
        await _reservations.SaveChangesAsync(cancellationToken);

        return reservation.Id;
    }
}
