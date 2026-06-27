using Ceiba.LiveEvent.Reservations.Domain.Common;

namespace Ceiba.LiveEvent.Reservations.Domain.Reservations.Events;

/// <summary>Se dispara cuando se crea una reserva en estado pendiente de pago (RF-03).</summary>
public sealed class ReservationCreatedEvent : BaseEvent
{
    public ReservationCreatedEvent(Reservation reservation) => Reservation = reservation;

    public Reservation Reservation { get; }
}
