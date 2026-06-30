using Ceiba.LiveEvent.Reservations.Application.Common.Exceptions;
using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Application.Reservations.Commands.CreateReservation;
using Ceiba.LiveEvent.Reservations.Application.Tests.Common;
using Ceiba.LiveEvent.Reservations.Domain.Events;
using Ceiba.LiveEvent.Reservations.Domain.Exceptions;
using Ceiba.LiveEvent.Reservations.Domain.Reservations;
using FluentAssertions;
using Moq;

namespace Ceiba.LiveEvent.Reservations.Application.Tests.Reservations.Commands;

public class CreateReservationCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private readonly Mock<IReservationRepository> _reservations = new();
    private readonly Mock<IEventRepository> _events = new();
    private readonly Mock<IEventStatusRepository> _eventStatuses = new();
    private readonly Mock<IReservationStatusRepository> _reservationStatuses = new();
    private readonly FakeDateTimeProvider _clock = new(Now);

    private readonly Guid _activeId = Guid.NewGuid();
    private readonly Guid _cancelledEventId = Guid.NewGuid();
    private readonly Guid _pendingId = Guid.NewGuid();
    private readonly Guid _confirmedId = Guid.NewGuid();

    public CreateReservationCommandHandlerTests()
    {
        _eventStatuses.Setup(s => s.GetByNameAsync("Activo", It.IsAny<CancellationToken>()))
            .ReturnsAsync(EventStatusLookup.Create("activo", _activeId));
        _reservationStatuses.Setup(s => s.GetByNameAsync(ReservationStatusNames.PendingPayment, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ReservationStatusLookup.Create("pendiente_pago", _pendingId));
        _reservationStatuses.Setup(s => s.GetByNameAsync(ReservationStatusNames.Confirmed, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ReservationStatusLookup.Create("confirmada", _confirmedId));
    }

    private CreateReservationCommandHandler CreateHandler()
        => new(_reservations.Object, _events.Object, _eventStatuses.Object, _reservationStatuses.Object, _clock);

    private Event ActiveEvent(int capacity = 100)
        => Event.Create("Evento", "Descripción del evento.", Guid.NewGuid(), 1000, capacity,
            Now.AddDays(10), Now.AddDays(10).AddHours(2), 20m, Guid.NewGuid(), _activeId, Now);

    private CreateReservationCommand Command(Guid eventId, int qty = 2) => new()
    {
        EventId = eventId,
        Quantity = qty,
        PurchaserName = "Ana Pérez",
        PurchaserEmail = "ana@example.com"
    };

    [Fact]
    public async Task Handle_ConEventoActivoYDisponibilidad_CreaReserva()
    {
        var @event = ActiveEvent();
        _events.Setup(e => e.GetByIdAsync(@event.Id, It.IsAny<CancellationToken>())).ReturnsAsync(@event);
        _reservations.Setup(r => r.GetByEventAsync(@event.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reservation>());

        Reservation? added = null;
        _reservations.Setup(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()))
            .Callback<Reservation, CancellationToken>((r, _) => added = r)
            .Returns(Task.CompletedTask);

        var id = await CreateHandler().Handle(Command(@event.Id), CancellationToken.None);

        added.Should().NotBeNull();
        id.Should().Be(added!.Id);
        // Estado inicial = id de "pendiente_pago" resuelto del catálogo.
        added.ReservationStatusId.Should().Be(_pendingId);
        _reservations.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CuandoEventoNoExiste_LanzaNotFound()
    {
        _events.Setup(e => e.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);

        var act = () => CreateHandler().Handle(Command(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CuandoEventoNoEstaActivo_LanzaDomainException()
    {
        var @event = ActiveEvent();
        @event.Cancel(_cancelledEventId);   // su EventStatusId deja de ser el de "activo"
        _events.Setup(e => e.GetByIdAsync(@event.Id, It.IsAny<CancellationToken>())).ReturnsAsync(@event);

        var act = () => CreateHandler().Handle(Command(@event.Id), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_CuandoNoHayDisponibilidad_LanzaDomainException()
    {
        var @event = ActiveEvent(capacity: 5);
        var existing = Reservation.Create(@event.Id, 5, "Y", "y@example.com", 5, Now.AddDays(10), 20m, Now, _pendingId);

        _events.Setup(e => e.GetByIdAsync(@event.Id, It.IsAny<CancellationToken>())).ReturnsAsync(@event);
        _reservations.Setup(r => r.GetByEventAsync(@event.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reservation> { existing });

        var act = () => CreateHandler().Handle(Command(@event.Id, qty: 1), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }
}
