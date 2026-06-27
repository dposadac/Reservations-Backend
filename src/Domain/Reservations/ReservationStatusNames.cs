namespace Ceiba.LiveEvent.Reservations.Domain.Reservations;

/// <summary>
/// Nombres de los estados de reserva tal como están en el catálogo <c>reservation_status</c>.
/// El enum <see cref="ReservationStatus"/> no coincide 1:1 con esos nombres
/// (p. ej. <c>PendientePago</c> ↔ <c>pendiente_pago</c>), por eso se centralizan aquí.
/// </summary>
public static class ReservationStatusNames
{
    public const string PendingPayment = "pendiente_pago";
    public const string Confirmed = "confirmada";
    public const string Cancelled = "cancelada";

    /// <summary>Devuelve el nombre de catálogo correspondiente a un valor del enum.</summary>
    public static string For(ReservationStatus status) => status switch
    {
        ReservationStatus.PendientePago => PendingPayment,
        ReservationStatus.Confirmada => Confirmed,
        ReservationStatus.Cancelada => Cancelled,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Estado de reserva no soportado."),
    };
}
