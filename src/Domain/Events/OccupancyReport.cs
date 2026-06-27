using Ceiba.LiveEvent.Reservations.Domain.Reservations;

namespace Ceiba.LiveEvent.Reservations.Domain.Events;

/// <summary>
/// Reporte de ocupación de un evento (RF-06). Value object inmutable calculado a partir
/// del evento y sus reservas.
/// </summary>
public sealed record OccupancyReport
{
    private OccupancyReport(
        Guid eventId,
        int maximumCapacity,
        int ticketsSold,
        int availableTickets,
        decimal occupancyPercentage,
        decimal totalRevenue,
        Guid eventStatusId)
    {
        EventId = eventId;
        MaximumCapacity = maximumCapacity;
        TicketsSold = ticketsSold;
        AvailableTickets = availableTickets;
        OccupancyPercentage = occupancyPercentage;
        TotalRevenue = totalRevenue;
        EventStatusId = eventStatusId;
    }

    public Guid EventId { get; }

    public int MaximumCapacity { get; }

    /// <summary>Total de entradas vendidas (confirmadas).</summary>
    public int TicketsSold { get; }

    /// <summary>Entradas disponibles restantes, excluyendo las perdidas por penalización (RN-07).</summary>
    public int AvailableTickets { get; }

    /// <summary>Porcentaje de ocupación (vendidas / capacidad).</summary>
    public decimal OccupancyPercentage { get; }

    /// <summary>Ingresos totales (precio × entradas confirmadas).</summary>
    public decimal TotalRevenue { get; }

    /// <summary>Estado del evento (FK al catálogo <c>event_status</c>).</summary>
    public Guid EventStatusId { get; }

    /// <summary>Construye el reporte de ocupación de un evento a partir de sus reservas.</summary>
    /// <param name="confirmedStatusId">Id del estado "confirmada" para contar las ventas.</param>
    public static OccupancyReport Create(
        Event @event,
        IEnumerable<Reservation> reservations,
        Guid confirmedStatusId)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(reservations);

        var list = reservations as ICollection<Reservation> ?? reservations.ToList();

        var sold = ReservationTicketing.ConfirmedTickets(list, confirmedStatusId);
        var forfeited = ReservationTicketing.ForfeitedTickets(list);
        var available = Math.Max(@event.MaximumCapacity - sold - forfeited, 0);

        var occupancy = @event.MaximumCapacity > 0
            ? Math.Round((decimal)sold / @event.MaximumCapacity * 100m, 2)
            : 0m;

        var revenue = sold * @event.TicketPrice;

        return new OccupancyReport(
            @event.Id,
            @event.MaximumCapacity,
            sold,
            available,
            occupancy,
            revenue,
            @event.EventStatusId);
    }
}
