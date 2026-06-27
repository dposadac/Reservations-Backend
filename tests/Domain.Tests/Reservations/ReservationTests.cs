using Ceiba.LiveEvent.Reservations.Domain.Exceptions;
using Ceiba.LiveEvent.Reservations.Domain.Reservations;
using Ceiba.LiveEvent.Reservations.Domain.Reservations.Events;
using FluentAssertions;

namespace Ceiba.LiveEvent.Reservations.Domain.Tests.Reservations;

public class ReservationTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly Guid EventId = Guid.NewGuid();
    private static readonly Guid PendingId = Guid.NewGuid();
    private static readonly Guid ConfirmedId = Guid.NewGuid();
    private static readonly Guid CancelledId = Guid.NewGuid();

    private static Reservation CreateValid(
        int quantity = 2,
        int availableTickets = 100,
        DateTimeOffset? eventStart = null,
        decimal ticketPrice = 50m)
        => Reservation.Create(
            EventId,
            quantity,
            "Ana Pérez",
            "ana@example.com",
            availableTickets,
            eventStart ?? Now.AddDays(10),
            ticketPrice,
            Now,
            PendingId);

    private static void Confirm(Reservation reservation)
        => reservation.ConfirmPayment(ReservationCode.New(), ConfirmedId, CancelledId);

    private static void Cancel(Reservation reservation, DateTimeOffset? eventStart = null)
        => reservation.Cancel(Now, CancelledId, PendingId, eventStart ?? Now.AddDays(10));

    [Fact]
    public void Create_ConDatosValidos_EstableceEstadoPendienteYRegistraEvento()
    {
        var reservation = CreateValid();

        reservation.ReservationStatusId.Should().Be(PendingId);
        reservation.Quantity.Should().Be(2);
        reservation.PurchaserEmail.Value.Should().Be("ana@example.com");
        reservation.Code.Should().BeNull();
        reservation.DomainEvents.Should().ContainSingle(e => e is ReservationCreatedEvent);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ConCantidadMenorQueUno_LanzaDomainException(int quantity)
    {
        var act = () => CreateValid(quantity: quantity);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ConEmailInvalido_LanzaDomainException()
    {
        var act = () => Reservation.Create(EventId, 1, "Ana", "no-es-email", 10, Now.AddDays(5), 50m, Now, PendingId);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_CuandoExcedeDisponibilidad_LanzaDomainException()
    {
        var act = () => CreateValid(quantity: 5, availableTickets: 4);

        act.Should().Throw<DomainException>().WithMessage("*disponibles*");
    }

    [Fact]
    public void Create_AMenosDe24Horas_ConMasDe5Entradas_LanzaDomainException()
    {
        var act = () => CreateValid(quantity: 6, availableTickets: 100, eventStart: Now.AddHours(10));

        act.Should().Throw<DomainException>().WithMessage("*24 horas*");
    }

    [Fact]
    public void Create_AMenosDe24Horas_ConHasta5Entradas_EsValido()
    {
        var reservation = CreateValid(quantity: 5, availableTickets: 100, eventStart: Now.AddHours(10));

        reservation.Quantity.Should().Be(5);
    }

    [Fact] // RN-04
    public void Create_CuandoFaltaMenosDeUnaHora_LanzaBusinessRuleException()
    {
        var act = () => CreateValid(quantity: 1, eventStart: Now.AddMinutes(30));

        act.Should().Throw<BusinessRuleException>()
            .Which.MessageKey.Should().Be("rules.rn04LateReservation");
    }

    [Fact] // RN-05
    public void Create_ConPrecioAltoYMasDe10Entradas_LanzaBusinessRuleException()
    {
        var act = () => CreateValid(quantity: 11, availableTickets: 100, ticketPrice: 150m);

        act.Should().Throw<BusinessRuleException>()
            .Which.MessageKey.Should().Be("rules.rn05PriceTicketLimit");
    }

    [Fact] // RN-05
    public void Create_ConPrecioAltoYHasta10Entradas_EsValido()
    {
        var reservation = CreateValid(quantity: 10, availableTickets: 100, ticketPrice: 150m);

        reservation.Quantity.Should().Be(10);
    }

    [Fact]
    public void ConfirmPayment_DesdePendiente_ConfirmaYAsignaCodigo()
    {
        var reservation = CreateValid();
        reservation.ClearDomainEvents();
        var code = ReservationCode.New();

        reservation.ConfirmPayment(code, ConfirmedId, CancelledId);

        reservation.ReservationStatusId.Should().Be(ConfirmedId);
        reservation.Code.Should().Be(code);
        reservation.DomainEvents.Should().ContainSingle(e => e is ReservationConfirmedEvent);
    }

    [Fact]
    public void ConfirmPayment_CuandoYaEstaConfirmada_LanzaDomainException()
    {
        var reservation = CreateValid();
        Confirm(reservation);

        var act = () => Confirm(reservation);

        act.Should().Throw<DomainException>().WithMessage("*ya está confirmada*");
    }

    [Fact]
    public void ConfirmPayment_CuandoEstaCancelada_LanzaDomainException()
    {
        var reservation = CreateValid();
        Confirm(reservation);
        Cancel(reservation);

        var act = () => Confirm(reservation);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_DesdeConfirmada_PasaACanceladaYRegistraFecha()
    {
        var reservation = CreateValid();
        Confirm(reservation);
        reservation.ClearDomainEvents();

        // Evento lejano (> 48h): sin penalización.
        Cancel(reservation, eventStart: Now.AddDays(10));

        reservation.ReservationStatusId.Should().Be(CancelledId);
        reservation.CancelledAt.Should().Be(Now);
        reservation.IsForfeited.Should().BeFalse();
        reservation.DomainEvents.Should().ContainSingle(e => e is ReservationCancelledEvent);
    }

    [Fact] // RN-07
    public void Cancel_AMenosDe48Horas_MarcaEntradasComoPerdidas()
    {
        var reservation = CreateValid();
        Confirm(reservation);

        Cancel(reservation, eventStart: Now.AddHours(40));

        reservation.IsForfeited.Should().BeTrue();
    }

    [Fact] // RN-07
    public void Cancel_AMasDe48Horas_NoPenaliza()
    {
        var reservation = CreateValid();
        Confirm(reservation);

        Cancel(reservation, eventStart: Now.AddHours(72));

        reservation.IsForfeited.Should().BeFalse();
    }

    [Fact]
    public void Cancel_CuandoEstaPendiente_LanzaDomainException()
    {
        var reservation = CreateValid();

        var act = () => Cancel(reservation);

        act.Should().Throw<DomainException>().WithMessage("*pendiente de pago*");
    }

    [Fact]
    public void Cancel_CuandoYaEstaCancelada_LanzaDomainException()
    {
        var reservation = CreateValid();
        Confirm(reservation);
        Cancel(reservation);

        var act = () => Cancel(reservation);

        act.Should().Throw<DomainException>().WithMessage("*ya está cancelada*");
    }
}
