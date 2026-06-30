using System.Text.RegularExpressions;
using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Application.Reservations.Commands.ConfirmReservationPayment;
using Ceiba.LiveEvent.Reservations.Domain.Reservations;
using FluentAssertions;
using Moq;

namespace Ceiba.LiveEvent.Reservations.Application.Tests.Reservations.Commands;

public class ConfirmReservationPaymentCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private readonly Mock<IReservationRepository> _reservations = new();
    private readonly Mock<IReservationStatusRepository> _reservationStatuses = new();

    private readonly Guid _pendingId = Guid.NewGuid();
    private readonly Guid _confirmedId = Guid.NewGuid();
    private readonly Guid _cancelledId = Guid.NewGuid();

    public ConfirmReservationPaymentCommandHandlerTests()
    {
        _reservationStatuses.Setup(s => s.GetByNameAsync(ReservationStatusNames.Confirmed, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ReservationStatusLookup.Create("confirmada", _confirmedId));
        _reservationStatuses.Setup(s => s.GetByNameAsync(ReservationStatusNames.Cancelled, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ReservationStatusLookup.Create("cancelada", _cancelledId));
    }

    private ConfirmReservationPaymentCommandHandler CreateHandler()
        => new(_reservations.Object, _reservationStatuses.Object);

    private Reservation PendingReservation()
        => Reservation.Create(Guid.NewGuid(), 2, "Ana", "ana@example.com", 100, Now.AddDays(10), 50m, Now, _pendingId);

    [Fact]
    public async Task Handle_ConfirmaYDevuelveCodigoConFormato()
    {
        var reservation = PendingReservation();
        _reservations.Setup(r => r.GetByIdAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);
        _reservations.Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var code = await CreateHandler().Handle(new ConfirmReservationPaymentCommand(reservation.Id), CancellationToken.None);

        Regex.IsMatch(code, @"^EV-\d{6}$").Should().BeTrue();
        reservation.ReservationStatusId.Should().Be(_confirmedId);
        reservation.Code!.Value.Should().Be(code);
        _reservations.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CuandoCodigoColisiona_ReintentaHastaObtenerUnoUnico()
    {
        var reservation = PendingReservation();
        _reservations.Setup(r => r.GetByIdAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        // Primer código existe, el segundo no -> debe reintentar.
        _reservations.SetupSequence(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        var code = await CreateHandler().Handle(new ConfirmReservationPaymentCommand(reservation.Id), CancellationToken.None);

        code.Should().MatchRegex(@"^EV-\d{6}$");
        _reservations.Verify(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
