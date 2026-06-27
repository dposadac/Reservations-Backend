using Ceiba.LiveEvent.Reservations.Domain.Common;

namespace Ceiba.LiveEvent.Reservations.Domain.Reservations.Events;

/// <summary>Se dispara cuando se confirma el pago de una reserva (RF-04).</summary>
public sealed class ReservationConfirmedEvent : BaseEvent
{
    public ReservationConfirmedEvent(Reservation reservation) => Reservation = reservation;

    public Reservation Reservation { get; }
}
