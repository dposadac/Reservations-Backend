using Ceiba.LiveEvent.Reservations.Application.Common.Exceptions;
using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Application.Events.Commands.CreateEvent;
using Ceiba.LiveEvent.Reservations.Application.Tests.Common;
using Ceiba.LiveEvent.Reservations.Domain.Events;
using Ceiba.LiveEvent.Reservations.Domain.Venues;
using FluentAssertions;
using Moq;

namespace Ceiba.LiveEvent.Reservations.Application.Tests.Events.Commands;

public class CreateEventCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private readonly Mock<IEventRepository> _events = new();
    private readonly Mock<IVenueRepository> _venues = new();
    private readonly Mock<IEventTypeRepository> _eventTypes = new();
    private readonly Mock<IEventStatusRepository> _eventStatuses = new();
    private readonly FakeDateTimeProvider _clock = new(Now);

    private readonly Guid _activeStatusId = Guid.NewGuid();

    public CreateEventCommandHandlerTests()
    {
        // Por defecto, los catálogos resuelven cualquier tipo/estado (las pruebas lo ajustan).
        _eventTypes
            .Setup(t => t.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EventTypeLookup.Create("conferencia"));
        _eventStatuses
            .Setup(s => s.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EventStatusLookup.Create("activo", _activeStatusId));
        // RN-02: por defecto el venue no tiene eventos (sin superposición).
        _events
            .Setup(e => e.GetByVenueAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Event>());
    }

    private CreateEventCommandHandler CreateHandler()
        => new(_events.Object, _venues.Object, _eventTypes.Object, _eventStatuses.Object, _clock);

    private CreateEventCommand ValidCommand(Guid venueId, int capacity = 100) => new()
    {
        Title = "Concierto de prueba",
        Description = "Una descripción válida para el evento.",
        VenueId = venueId,
        MaximumCapacity = capacity,
        StartDate = Now.AddDays(10),
        EndDate = Now.AddDays(10).AddHours(3),
        TicketPrice = 50m,
        Type = EventType.Conferencia
    };

    [Fact]
    public async Task Handle_ConVenueExistente_CreaEventoYDevuelveId()
    {
        var venue = Venue.Create("Auditorio", 200, "Bogotá");
        _venues.Setup(v => v.GetByIdAsync(venue.Id, It.IsAny<CancellationToken>())).ReturnsAsync(venue);

        Event? added = null;
        _events.Setup(e => e.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
            .Callback<Event, CancellationToken>((e, _) => added = e)
            .Returns(Task.CompletedTask);

        var id = await CreateHandler().Handle(ValidCommand(venue.Id), CancellationToken.None);

        added.Should().NotBeNull();
        id.Should().Be(added!.Id);
        // Estado inicial = id de "activo" resuelto del catálogo.
        added.EventStatusId.Should().Be(_activeStatusId);
        _events.Verify(e => e.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        // Tipo y estado se resolvieron filtrando el catálogo por su nombre (texto).
        _eventTypes.Verify(t => t.GetByNameAsync("Conferencia", It.IsAny<CancellationToken>()), Times.Once);
        _eventStatuses.Verify(s => s.GetByNameAsync("Activo", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CuandoVenueNoExiste_LanzaNotFound()
    {
        _venues.Setup(v => v.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Venue?)null);

        var act = () => CreateHandler().Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CuandoTipoNoEstaEnElCatalogo_LanzaNotFound()
    {
        var venue = Venue.Create("Auditorio", 200, "Bogotá");
        _venues.Setup(v => v.GetByIdAsync(venue.Id, It.IsAny<CancellationToken>())).ReturnsAsync(venue);
        _eventTypes.Setup(t => t.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventTypeLookup?)null);

        var act = () => CreateHandler().Handle(ValidCommand(venue.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CuandoCapacidadSuperaElVenue_PropagaDomainException()
    {
        var venue = Venue.Create("Sala", 50, "Bogotá");
        _venues.Setup(v => v.GetByIdAsync(venue.Id, It.IsAny<CancellationToken>())).ReturnsAsync(venue);

        var act = () => CreateHandler().Handle(ValidCommand(venue.Id, capacity: 100), CancellationToken.None);

        await act.Should().ThrowAsync<Domain.Exceptions.DomainException>();
    }

    [Fact] // RN-02
    public async Task Handle_CuandoVenueTieneEventoSuperpuesto_LanzaBusinessRuleException()
    {
        var venue = Venue.Create("Auditorio", 200, "Bogotá");
        _venues.Setup(v => v.GetByIdAsync(venue.Id, It.IsAny<CancellationToken>())).ReturnsAsync(venue);

        // Evento activo existente que se solapa con el horario del comando (día 10 + 3h).
        var overlapping = Event.Create("Otro evento", "Descripción del evento.", venue.Id, 200, 100,
            Now.AddDays(10).AddHours(1), Now.AddDays(10).AddHours(2), 10m, Guid.NewGuid(), _activeStatusId, Now);
        _events.Setup(e => e.GetByVenueAsync(venue.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Event> { overlapping });

        var act = () => CreateHandler().Handle(ValidCommand(venue.Id), CancellationToken.None);

        await act.Should().ThrowAsync<Domain.Exceptions.BusinessRuleException>();
    }
}
