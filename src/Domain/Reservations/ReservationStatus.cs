namespace Ceiba.LiveEvent.Reservations.Domain.Reservations;

/// <summary>
/// Estado de una reserva. Persiste como FK a la tabla maestra
/// <c>reservation_status</c> mediante UUID fijos (ver convertidor en infraestructura).
/// </summary>
public enum ReservationStatus
{
    PendientePago = 1,
    Confirmada = 2,
    Cancelada = 3
}
