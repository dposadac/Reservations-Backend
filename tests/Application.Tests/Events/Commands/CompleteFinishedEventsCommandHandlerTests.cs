using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Application.Events.Commands.CompleteFinishedEvents;
using Ceiba.LiveEvent.Reservations.Application.Tests.Common;
using Ceiba.LiveEvent.Reservations.Domain.Events;
using FluentAssertions;
using Moq;

namespace Ceiba.LiveEvent.Reservations.Application.Tests.Events.Commands;

public class CompleteFinishedEventsCommandHandlerTests
{
    private static readonly DateTimeOffset Base = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private readonly Mock<IEventRepository> _events = new();
    private readonly Mock<IEventStatusRepository> _eventStatuses = new();
    // El reloj "actual" está 30 días después de la base para simular eventos finalizados.
    private readonly FakeDateTimeProvider _clock = new(Base.AddDays(30));

    private readonly Guid _activeId = Guid.NewGuid();
    private readonly Guid _completedId = Guid.NewGuid();

    public CompleteFinishedEventsCommandHandlerTests()
    {
        _eventStatuses.Setup(s => s.GetByNameAsync("Activo", It.IsAny<CancellationToken>()))
            .ReturnsAsync(EventStatusLookup.Create("activo", _activeId));
        _eventStatuses.Setup(s => s.GetByNameAsync("Completado", It.IsAny<CancellationToken>()))
            .ReturnsAsync(EventStatusLookup.Create("completado", _completedId));
    }

    private CompleteFinishedEventsCommandHandler CreateHandler()
        => new(_events.Object, _eventStatuses.Object, _clock);

    private Event BuildEvent(int startDays, Guid statusId)
        => Event.Create("Evento", "Descripción del evento.", Guid.NewGuid(), 1000, 100,
            Base.AddDays(startDays), Base.AddDays(startDays).AddHours(2), 10m, Guid.NewGuid(), statusId, Base);

    [Fact]
    public async Task Handle_CompletaSoloLosEventosActivosFinalizados()
    {
        var endedActive = BuildEvent(startDays: 10, statusId: _activeId);   // fin = base+10d < ahora
        var futureActive = BuildEvent(startDays: 40, statusId: _activeId);  // fin = base+40d > ahora

        // Evento ya completado (no debe contarse ni reprocesarse).
        var alreadyCompleted = BuildEvent(startDays: 5, statusId: _activeId);
        alreadyCompleted.RefreshStatus(_activeId, _completedId, Base.AddDays(6));

        _events.Setup(e => e.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Event> { endedActive, futureActive, alreadyCompleted });

        var count = await CreateHandler().Handle(new CompleteFinishedEventsCommand(), CancellationToken.None);

        count.Should().Be(1);
        endedActive.EventStatusId.Should().Be(_completedId);
        futureActive.EventStatusId.Should().Be(_activeId);
        _events.Verify(e => e.Update(endedActive), Times.Once);
        _events.Verify(e => e.Update(futureActive), Times.Never);
        _events.Verify(e => e.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CuandoNoHayFinalizados_NoGuardaYDevuelveCero()
    {
        var futureActive = BuildEvent(startDays: 40, statusId: _activeId);
        _events.Setup(e => e.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Event> { futureActive });

        var count = await CreateHandler().Handle(new CompleteFinishedEventsCommand(), CancellationToken.None);

        count.Should().Be(0);
        _events.Verify(e => e.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
