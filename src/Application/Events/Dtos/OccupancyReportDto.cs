using Ceiba.LiveEvent.Reservations.Domain.Events;

namespace Ceiba.LiveEvent.Reservations.Application.Events.Dtos;

/// <summary>Proyección de lectura del reporte de ocupación de un evento (RF-06).</summary>
public sealed record OccupancyReportDto
{
    public Guid EventId { get; init; }

    public int MaximumCapacity { get; init; }

    public int TicketsSold { get; init; }

    public int AvailableTickets { get; init; }

    public decimal OccupancyPercentage { get; init; }

    public decimal TotalRevenue { get; init; }

    public string EventStatusName { get; init; } = string.Empty;

    public static OccupancyReportDto FromReport(OccupancyReport report, string eventStatusName = "") => new()
    {
        EventId = report.EventId,
        MaximumCapacity = report.MaximumCapacity,
        TicketsSold = report.TicketsSold,
        AvailableTickets = report.AvailableTickets,
        OccupancyPercentage = report.OccupancyPercentage,
        TotalRevenue = report.TotalRevenue,
        EventStatusName = eventStatusName
    };
}
