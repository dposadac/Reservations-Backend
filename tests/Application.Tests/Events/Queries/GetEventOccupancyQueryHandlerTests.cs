using Ceiba.LiveEvent.Reservations.Application.Common.Exceptions;
using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Application.Events.Queries.GetEventOccupancy;
using Ceiba.LiveEvent.Reservations.Application.Tests.Common;
using Ceiba.LiveEvent.Reservations.Domain.Events;
using Ceiba.LiveEvent.Reservations.Domain.Reservations;
using FluentAssertions;
using Moq;

namespace Ceiba.LiveEvent.Reservations.Application.Tests.Events.Queries;

public class GetEventOccupancyQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private readonly Mock<IEventRepository> _events = new();
    private readonly Mock<IReservationRepository> _reservations = new();
    private readonly Mock<IEventStatusRepository> _eventStatuses = new();
    private readonly Mock<IReservationStatusRepository> _reservationStatuses = new();
    private readonly FakeDateTimeProvider _clock = new(Now);

    private readonly Guid _activeId = Guid.NewGuid();
    private readonly Guid _completedId = Guid.NewGuid();
    private readonly Guid _pendingId = Guid.NewGuid();
    private readonly Guid _confirmedId = Guid.NewGuid();
    private readonly Guid _cancelledId = Guid.NewGuid();

    public GetEventOccupancyQueryHandlerTests()
    {
        _eventStatuses.Setup(s => s.GetByNameAsync("Activo", It.IsAny<CancellationToken>()))
            .ReturnsAsync(EventStatusLookup.Create("activo", _activeId));
        _eventStatuses.Setup(s => s.GetByNameAsync("Completado", It.IsAny<CancellationToken>()))
            .ReturnsAsync(EventStatusLookup.Create("completado", _completedId));
        _reservationStatuses.Setup(s => s.GetByNameAsync(ReservationStatusNames.Confirmed, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ReservationStatusLookup.Create("confirmada", _confirmedId));
    }

    private GetEventOccupancyQueryHandler CreateHandler()
        => new(_events.Object, _reservations.Object, _eventStatuses.Object, _reservationStatuses.Object, _clock);

    private Event ActiveEvent()
        => Event.Create("Evento", "Descripción del evento.", Guid.NewGuid(), 1000, 100,
            Now.AddDays(10), Now.AddDays(10).AddHours(2), 20m, Guid.NewGuid(), _activeId, Now);

    private Reservation Confirmed(Guid eventId, int qty)
    {
        var r = Reservation.Create(eventId, qty, "X", "x@example.com", 1000, Now.AddDays(10), 20m, Now, _pendingId);
        r.ConfirmPayment(ReservationCode.New(), _confirmedId, _cancelledId);
        return r;
    }

    [Fact]
    public async Task Handle_CuandoEventoNoExiste_LanzaNotFound()
    {
        _events.Setup(e => e.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);

        var act = () => CreateHandler().Handle(new GetEventOccupancyQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_DevuelveReporteConVendidasEIngresos()
    {
        var @event = ActiveEvent();
        var confirmed = Confirmed(@event.Id, 40);

        _events.Setup(e => e.GetByIdAsync(@event.Id, It.IsAny<CancellationToken>())).ReturnsAsync(@event);
        _reservations.Setup(r => r.GetByEventAsync(@event.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reservation> { confirmed });

        var report = await CreateHandler().Handle(new GetEventOccupancyQuery(@event.Id), CancellationToken.None);

        report.TicketsSold.Should().Be(40);
        report.AvailableTickets.Should().Be(60);
        report.TotalRevenue.Should().Be(800m);
        report.OccupancyPercentage.Should().Be(40m);
        report.EventStatusId.Should().Be(_activeId);
    }

    [Fact]
    public async Task Handle_CuandoEventoYaFinalizo_ReportaCompletado()
    {
        var @event = ActiveEvent();

        _events.Setup(e => e.GetByIdAsync(@event.Id, It.IsAny<CancellationToken>())).ReturnsAsync(@event);
        _reservations.Setup(r => r.GetByEventAsync(@event.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reservation>());

        _clock.UtcNow = Now.AddDays(30); // posterior a la fecha de fin

        var report = await CreateHandler().Handle(new GetEventOccupancyQuery(@event.Id), CancellationToken.None);

        report.EventStatusId.Should().Be(_completedId);
    }
}
