using Ceiba.LiveEvent.Reservations.Domain.Events;
using Ceiba.LiveEvent.Reservations.Domain.Events.Events;
using Ceiba.LiveEvent.Reservations.Domain.Exceptions;
using FluentAssertions;

namespace Ceiba.LiveEvent.Reservations.Domain.Tests.Events;

public class EventTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly Guid VenueId = Guid.NewGuid();
    private static readonly Guid EventTypeId = Guid.NewGuid();
    private static readonly Guid ActiveId = Guid.NewGuid();
    private static readonly Guid CancelledId = Guid.NewGuid();
    private static readonly Guid CompletedId = Guid.NewGuid();

    private static Event CreateValid(
        int maximumCapacity = 100,
        int venueCapacity = 200,
        decimal ticketPrice = 50m,
        Guid? eventTypeId = null,
        Guid? eventStatusId = null)
        => Event.Create(
            "Concierto de prueba",
            "Una descripción válida para el evento.",
            VenueId,
            venueCapacity,
            maximumCapacity,
            Now.AddDays(10),
            Now.AddDays(10).AddHours(3),
            ticketPrice,
            eventTypeId ?? EventTypeId,
            eventStatusId ?? ActiveId,
            Now);

    [Fact]
    public void Create_ConDatosValidos_EstableceEstadoActivoYRegistraEvento()
    {
        var @event = CreateValid();

        @event.Title.Should().Be("Concierto de prueba");
        @event.EventStatusId.Should().Be(ActiveId);
        @event.EventTypeId.Should().Be(EventTypeId);
        @event.Id.Should().NotBe(Guid.Empty);
        @event.DomainEvents.Should().ContainSingle(e => e is EventCreatedEvent);
    }

    [Theory]
    [InlineData("1234")]            // 4 < 5
    [InlineData("")]
    public void Create_ConTituloMuyCorto_LanzaDomainException(string title)
    {
        var act = () => Event.Create(title, "Descripción válida xxx", VenueId, 200, 100,
            Now.AddDays(10), Now.AddDays(10).AddHours(2), 10m, EventTypeId, ActiveId, Now);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ConDescripcionMuyCorta_LanzaDomainException()
    {
        var act = () => Event.Create("Título válido", "corta", VenueId, 200, 100,
            Now.AddDays(10), Now.AddDays(10).AddHours(2), 10m, EventTypeId, ActiveId, Now);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ConTipoVacio_LanzaDomainException()
    {
        var act = () => CreateValid(eventTypeId: Guid.Empty);

        act.Should().Throw<DomainException>().WithMessage("*tipo de evento*");
    }

    [Fact]
    public void Create_ConEstadoVacio_LanzaDomainException()
    {
        var act = () => CreateValid(eventStatusId: Guid.Empty);

        act.Should().Throw<DomainException>().WithMessage("*estado del evento*");
    }

    [Fact] // RN-01
    public void Create_CuandoCapacidadSuperaLaDelVenue_LanzaBusinessRuleException()
    {
        var act = () => CreateValid(maximumCapacity: 300, venueCapacity: 200);

        act.Should().Throw<BusinessRuleException>()
            .Which.MessageKey.Should().Be("rules.rn01VenueCapacity");
    }

    [Fact] // RN-03
    public void Create_EnFinDeSemanaDespuesDe22_LanzaBusinessRuleException()
    {
        // 2026-01-03 es sábado; inicio a las 23:00.
        var saturdayNight = new DateTimeOffset(2026, 1, 3, 23, 0, 0, TimeSpan.Zero);
        var act = () => Event.Create("Título válido", "Descripción válida xx", VenueId, 200, 100,
            saturdayNight, saturdayNight.AddHours(2), 10m, EventTypeId, ActiveId, Now);

        act.Should().Throw<BusinessRuleException>()
            .Which.MessageKey.Should().Be("rules.rn03WeekendNight");
    }

    [Fact]
    public void Create_ConCapacidadNoPositiva_LanzaDomainException()
    {
        var act = () => CreateValid(maximumCapacity: 0);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ConFechaInicioPasada_LanzaDomainException()
    {
        var act = () => Event.Create("Título válido", "Descripción válida xx", VenueId, 200, 100,
            Now.AddDays(-1), Now.AddDays(1), 10m, EventTypeId, ActiveId, Now);

        act.Should().Throw<DomainException>().WithMessage("*futura*");
    }

    [Fact]
    public void Create_ConFechaFinAnteriorAlInicio_LanzaDomainException()
    {
        var act = () => Event.Create("Título válido", "Descripción válida xx", VenueId, 200, 100,
            Now.AddDays(10), Now.AddDays(9), 10m, EventTypeId, ActiveId, Now);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ConPrecioNoPositivo_LanzaDomainException()
    {
        var act = () => CreateValid(ticketPrice: 0m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_DesdeActivo_PasaACanceladoYRegistraEvento()
    {
        var @event = CreateValid();
        @event.ClearDomainEvents();

        @event.Cancel(CancelledId);

        @event.EventStatusId.Should().Be(CancelledId);
        @event.DomainEvents.Should().ContainSingle(e => e is EventCancelledEvent);
    }

    [Fact]
    public void Cancel_CuandoYaEstaCancelado_LanzaDomainException()
    {
        var @event = CreateValid();
        @event.Cancel(CancelledId);

        var act = () => @event.Cancel(CancelledId);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RefreshStatus_CuandoYaFinalizo_PasaACompletado()
    {
        var @event = CreateValid();

        @event.RefreshStatus(ActiveId, CompletedId, Now.AddDays(20));

        @event.EventStatusId.Should().Be(CompletedId);
    }

    [Fact]
    public void RefreshStatus_AntesDeFinalizar_MantieneActivo()
    {
        var @event = CreateValid();

        @event.RefreshStatus(ActiveId, CompletedId, Now.AddDays(10));

        @event.EventStatusId.Should().Be(ActiveId);
    }
}
