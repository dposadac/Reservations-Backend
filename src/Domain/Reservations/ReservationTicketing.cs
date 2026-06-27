namespace Ceiba.LiveEvent.Reservations.Domain.Reservations;

/// <summary>
/// Cálculos de aforo a partir del conjunto de reservas de un evento. Servicio de dominio
/// sin estado, usado para validar disponibilidad (RF-03) y construir reportes (RF-06).
/// Los ids de los estados los resuelve el llamador contra el catálogo.
/// </summary>
public static class ReservationTicketing
{
    /// <summary>Entradas confirmadas (vendidas).</summary>
    public static int ConfirmedTickets(IEnumerable<Reservation> reservations, Guid confirmedStatusId)
        => reservations.Where(r => r.ReservationStatusId == confirmedStatusId).Sum(r => r.Quantity);

    /// <summary>Entradas que ocupan aforo (pendientes de pago o confirmadas).</summary>
    public static int HeldTickets(IEnumerable<Reservation> reservations, Guid pendingStatusId, Guid confirmedStatusId)
        => reservations
            .Where(r => r.ReservationStatusId == pendingStatusId || r.ReservationStatusId == confirmedStatusId)
            .Sum(r => r.Quantity);

    /// <summary>Entradas perdidas por penalización (RN-07): no se liberan para la venta.</summary>
    public static int ForfeitedTickets(IEnumerable<Reservation> reservations)
        => reservations.Where(r => r.IsForfeited).Sum(r => r.Quantity);

    /// <summary>
    /// Entradas disponibles para reservar: capacidad menos las ocupadas (pendientes + confirmadas)
    /// y menos las perdidas por penalización. Nunca negativo.
    /// </summary>
    public static int AvailableForBooking(
        int maximumCapacity,
        IEnumerable<Reservation> reservations,
        Guid pendingStatusId,
        Guid confirmedStatusId)
    {
        var list = reservations as ICollection<Reservation> ?? reservations.ToList();
        var taken = HeldTickets(list, pendingStatusId, confirmedStatusId) + ForfeitedTickets(list);
        return Math.Max(maximumCapacity - taken, 0);
    }
}
