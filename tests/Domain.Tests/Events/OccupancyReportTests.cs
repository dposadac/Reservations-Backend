using Ceiba.LiveEvent.Reservations.Domain.Events;
using Ceiba.LiveEvent.Reservations.Domain.Reservations;
using FluentAssertions;

namespace Ceiba.LiveEvent.Reservations.Domain.Tests.Events;

public class OccupancyReportTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly Guid ActiveId = Guid.NewGuid();
    private static readonly Guid PendingId = Guid.NewGuid();
    private static readonly Guid ConfirmedId = Guid.NewGuid();
    private static readonly Guid CancelledId = Guid.NewGuid();

    private static Event CreateEvent(int capacity = 100, decimal price = 25m)
        => Event.Create("Evento ocupación", "Descripción del evento.", Guid.NewGuid(), 1000, capacity,
            Now.AddDays(10), Now.AddDays(10).AddHours(2), price, Guid.NewGuid(), ActiveId, Now);

    private static Reservation Confirmed(Guid eventId, int qty)
    {
        var r = Reservation.Create(eventId, qty, "X", "x@example.com", 1000, Now.AddDays(10), 25m, Now, PendingId);
        r.ConfirmPayment(ReservationCode.New(), ConfirmedId, CancelledId);
        return r;
    }

    private static Reservation Pending(Guid eventId, int qty)
        => Reservation.Create(eventId, qty, "X", "x@example.com", 1000, Now.AddDays(10), 25m, Now, PendingId);

    private static Reservation Forfeited(Guid eventId, int qty)
    {
        var r = Confirmed(eventId, qty);
        // Evento a menos de 48h -> la cancelación penaliza (RN-07).
        r.Cancel(Now, CancelledId, PendingId, Now.AddHours(1));
        return r;
    }

    [Fact]
    public void Create_CalculaVendidasDisponiblesOcupacionEIngresos()
    {
        var @event = CreateEvent(capacity: 100, price: 25m);
        var reservations = new[]
        {
            Confirmed(@event.Id, 30),
            Pending(@event.Id, 10),    // no cuenta como vendida
            Forfeited(@event.Id, 5),   // perdidas: no liberan ni venden
        };

        var report = OccupancyReport.Create(@event, reservations, ConfirmedId);

        report.TicketsSold.Should().Be(30);
        report.AvailableTickets.Should().Be(65);          // 100 - 30 vendidas - 5 perdidas
        report.OccupancyPercentage.Should().Be(30m);      // 30/100
        report.TotalRevenue.Should().Be(750m);            // 30 * 25
        report.EventStatusId.Should().Be(ActiveId);
    }

    [Fact]
    public void AvailableForBooking_DescuentaOcupadasYPerdidas()
    {
        var @event = CreateEvent(capacity: 50);
        var reservations = new[]
        {
            Confirmed(@event.Id, 10),
            Pending(@event.Id, 5),
            Forfeited(@event.Id, 5),
        };

        var available = ReservationTicketing.AvailableForBooking(
            @event.MaximumCapacity, reservations, PendingId, ConfirmedId);

        available.Should().Be(30); // 50 - (10 + 5) ocupadas - 5 perdidas
    }
}
