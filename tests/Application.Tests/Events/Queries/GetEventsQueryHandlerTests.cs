using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Application.Events.Queries.GetEvents;
using Ceiba.LiveEvent.Reservations.Domain.Events;
using FluentAssertions;
using Moq;

namespace Ceiba.LiveEvent.Reservations.Application.Tests.Events.Queries;

public class GetEventsQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private readonly Mock<IEventRepository> _events = new();
    private readonly Mock<IEventTypeRepository> _eventTypes = new();
    private readonly Mock<IEventStatusRepository> _eventStatuses = new();

    private readonly Guid _venueA = Guid.NewGuid();
    private readonly Guid _venueB = Guid.NewGuid();
    private readonly Guid _conciertoId = Guid.NewGuid();
    private readonly Guid _tallerId = Guid.NewGuid();
    private readonly Guid _conferenciaId = Guid.NewGuid();
    private static readonly Guid ActiveId = Guid.NewGuid();

    private GetEventsQueryHandler CreateHandler()
        => new(_events.Object, _eventTypes.Object, _eventStatuses.Object);

    private static Event Build(string title, Guid eventTypeId, Guid venueId, int daysToStart)
        => Event.Create(title, "Descripción del evento.", venueId, 1000, 100,
            Now.AddDays(daysToStart), Now.AddDays(daysToStart).AddHours(2), 10m, eventTypeId, ActiveId, Now);

    private void SetupCatalog() => _events
        .Setup(e => e.GetAllAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<Event>
        {
            Build("Concierto de Rock", _conciertoId, _venueA, 5),
            Build("Taller de Cocina", _tallerId, _venueB, 10),
            Build("Conferencia Tech", _conferenciaId, _venueA, 15),
        });

    [Fact]
    public async Task Handle_SinFiltros_DevuelveTodosOrdenadosPorFecha()
    {
        SetupCatalog();

        var result = await CreateHandler().Handle(new GetEventsQuery(), CancellationToken.None);

        result.Should().HaveCount(3);
        result.Select(r => r.StartDate).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Handle_FiltraPorTipo_ResolviendoElNombreContraElCatalogo()
    {
        SetupCatalog();
        // El handler resuelve "Taller" -> id del catálogo y filtra por EventTypeId.
        _eventTypes.Setup(t => t.GetByNameAsync("Taller", It.IsAny<CancellationToken>()))
            .ReturnsAsync(EventTypeLookup.Create("taller", _tallerId));

        var result = await CreateHandler().Handle(
            new GetEventsQuery { Type = EventType.Taller }, CancellationToken.None);

        result.Should().ContainSingle().Which.EventTypeId.Should().Be(_tallerId);
    }

    [Fact]
    public async Task Handle_FiltraPorTipo_CuandoElTipoNoExisteEnCatalogo_DevuelveVacio()
    {
        SetupCatalog();
        _eventTypes.Setup(t => t.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventTypeLookup?)null);

        var result = await CreateHandler().Handle(
            new GetEventsQuery { Type = EventType.Concierto }, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_FiltraPorVenue()
    {
        SetupCatalog();

        var result = await CreateHandler().Handle(
            new GetEventsQuery { VenueId = _venueA }, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.VenueId == _venueA);
    }

    [Fact]
    public async Task Handle_BusquedaPorTituloParcial_EsCaseInsensitive()
    {
        SetupCatalog();

        var result = await CreateHandler().Handle(
            new GetEventsQuery { Title = "concierto" }, CancellationToken.None);

        result.Should().ContainSingle().Which.Title.Should().Be("Concierto de Rock");
    }

    [Fact]
    public async Task Handle_FiltraPorRangoDeFechaDeInicio()
    {
        SetupCatalog();

        var result = await CreateHandler().Handle(
            new GetEventsQuery { StartDateFrom = Now.AddDays(8), StartDateTo = Now.AddDays(12) },
            CancellationToken.None);

        result.Should().ContainSingle().Which.Title.Should().Be("Taller de Cocina");
    }
}
