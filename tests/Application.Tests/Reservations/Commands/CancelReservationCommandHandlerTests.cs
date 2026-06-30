using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Application.Reservations.Commands.CancelReservation;
using Ceiba.LiveEvent.Reservations.Application.Tests.Common;
using Ceiba.LiveEvent.Reservations.Domain.Events;
using Ceiba.LiveEvent.Reservations.Domain.Reservations;
using FluentAssertions;
using Moq;

namespace Ceiba.LiveEvent.Reservations.Application.Tests.Reservations.Commands;

public class CancelReservationCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private readonly Mock<IReservationRepository> _reservations = new();
    private readonly Mock<IReservationStatusRepository> _reservationStatuses = new();
    private readonly Mock<IEventRepository> _events = new();
    private readonly FakeDateTimeProvider _clock = new(Now);

    private readonly Guid _pendingId = Guid.NewGuid();
    private readonly Guid _confirmedId = Guid.NewGuid();
    private readonly Guid _cancelledId = Guid.NewGuid();

    public CancelReservationCommandHandlerTests()
    {
        _reservationStatuses.Setup(s => s.GetByNameAsync(ReservationStatusNames.Cancelled, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ReservationStatusLookup.Create("cancelada", _cancelledId));
        _reservationStatuses.Setup(s => s.GetByNameAsync(ReservationStatusNames.PendingPayment, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ReservationStatusLookup.Create("pendiente_pago", _pendingId));
    }

    private CancelReservationCommandHandler CreateHandler()
        => new(_reservations.Object, _reservationStatuses.Object, _events.Object, _clock);

    // Evento cuyo inicio se sitúa a las horas indicadas desde "ahora" (controla RN-07).
    private Event EventStartingInHours(int hours)
        => Event.Create("Evento", "Descripción del evento.", Guid.NewGuid(), 1000, 100,
            Now.AddHours(hours), Now.AddHours(hours).AddHours(2), 20m, Guid.NewGuid(), Guid.NewGuid(), Now);

    private Reservation ConfirmedReservationFor(Event @event)
    {
        var r = Reservation.Create(@event.Id, 2, "Ana", "ana@example.com", 100, @event.StartDate, 20m, Now, _pendingId);
        r.ConfirmPayment(ReservationCode.New(), _confirmedId, _cancelledId);
        return r;
    }

    [Fact]
    public async Task Handle_CancelaReservaConfirmada_SinPenalizacionSiFaltanMasDe48h()
    {
        var @event = EventStartingInHours(72); // > 48h -> sin penalización
        var reservation = ConfirmedReservationFor(@event);
        _reservations.Setup(r => r.GetByIdAsync(reservation.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reservation);
        _events.Setup(e => e.GetByIdAsync(@event.Id, It.IsAny<CancellationToken>())).ReturnsAsync(@event);

        var penalized = await CreateHandler().Handle(new CancelReservationCommand(reservation.Id), CancellationToken.None);

        penalized.Should().BeFalse();
        reservation.ReservationStatusId.Should().Be(_cancelledId);
        reservation.CancelledAt.Should().Be(Now);
        reservation.IsForfeited.Should().BeFalse();
        _reservations.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact] // RN-07
    public async Task Handle_PenalizaSiFaltanMenosDe48h()
    {
        var @event = EventStartingInHours(40); // < 48h -> penaliza
        var reservation = ConfirmedReservationFor(@event);
        _reservations.Setup(r => r.GetByIdAsync(reservation.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reservation);
        _events.Setup(e => e.GetByIdAsync(@event.Id, It.IsAny<CancellationToken>())).ReturnsAsync(@event);

        var penalized = await CreateHandler().Handle(new CancelReservationCommand(reservation.Id), CancellationToken.None);

        penalized.Should().BeTrue();
        reservation.IsForfeited.Should().BeTrue();
    }
}
