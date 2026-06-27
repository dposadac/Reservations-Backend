using Ceiba.LiveEvent.Reservations.Domain.Common;

namespace Ceiba.LiveEvent.Reservations.Domain.Reservations.Events;

/// <summary>Se dispara cuando se cancela una reserva (RF-05).</summary>
public sealed class ReservationCancelledEvent : BaseEvent
{
    public ReservationCancelledEvent(Reservation reservation, bool penalized)
    {
        Reservation = reservation;
        Penalized = penalized;
    }

    public Reservation Reservation { get; }

    /// <summary>Indica si la cancelación aplicó penalización (entradas perdidas, RN-07).</summary>
    public bool Penalized { get; }
}
