using System.Net;
using System.Net.Http.Json;
using Ceiba.LiveEvent.Reservations.Api.Common;
using Ceiba.LiveEvent.Reservations.Application.Events.Commands.CreateEvent;
using Ceiba.LiveEvent.Reservations.Application.Reservations.Commands.CreateReservation;
using Ceiba.LiveEvent.Reservations.Domain.Events;
using FluentAssertions;

namespace Ceiba.LiveEvent.Reservations.Api.IntegrationTests;

/// <summary>
/// Verifica de extremo a extremo que las reglas de negocio (RN) se validan y exponen su
/// mensaje del catálogo (messages.json) cuando una petición las incumple.
/// </summary>
[Collection(nameof(DatabaseCollection))]
public class BusinessRulesTests
{
    private readonly HttpClient _client;

    public BusinessRulesTests(CustomWebApplicationFactory factory)
        => _client = factory.CreateClient();

    private static CreateEventCommand EventCmd(
        DateTimeOffset start,
        DateTimeOffset end,
        int capacity = 100,
        decimal price = 80m,
        string title = "Evento RN")
        => new()
        {
            Title = title,
            Description = "Descripción válida para la prueba de reglas.",
            VenueId = CustomWebApplicationFactory.TestVenueId,
            MaximumCapacity = capacity,
            StartDate = start,
            EndDate = end,
            TicketPrice = price,
            Type = EventType.Concierto
        };

    [Fact] // RN-01
    public async Task Rn01_CapacidadMayorQueElVenue_DevuelveMensaje()
    {
        var (start, end) = TestSchedule.NextSlot();
        var capacity = CustomWebApplicationFactory.TestVenueCapacity + 1;

        var response = await _client.PostAsJsonAsync("/api/event", EventCmd(start, end, capacity));
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        body.Should().Contain("capacidad del lugar");
    }

    [Fact] // RN-02
    public async Task Rn02_VenueConHorarioSuperpuesto_DevuelveMensaje()
    {
        var (start, end) = TestSchedule.NextSlot();

        var first = await _client.PostAsJsonAsync("/api/event", EventCmd(start, end, title: "Primero"));
        first.EnsureSuccessStatusCode();

        // Mismo venue y horario superpuesto -> RN-02.
        var second = await _client.PostAsJsonAsync("/api/event", EventCmd(start, end, title: "Segundo"));
        var body = await second.Content.ReadAsStringAsync();

        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        body.Should().Contain("horario superpuesto");
    }

    [Fact] // RN-03
    public async Task Rn03_FinDeSemanaDespuesDe22_DevuelveMensaje()
    {
        var day = DateTime.UtcNow.Date.AddDays(14);
        while (day.DayOfWeek != DayOfWeek.Saturday)
        {
            day = day.AddDays(1);
        }

        var saturdayNight = new DateTimeOffset(day.Year, day.Month, day.Day, 23, 0, 0, TimeSpan.Zero);

        var response = await _client.PostAsJsonAsync("/api/event", EventCmd(saturdayNight, saturdayNight.AddHours(2)));
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        body.Should().Contain("fin de semana");
    }

    [Fact] // RN-05
    public async Task Rn05_PrecioAltoYMasDe10Entradas_DevuelveMensaje()
    {
        var (start, end) = TestSchedule.NextSlot();
        var create = await _client.PostAsJsonAsync("/api/event", EventCmd(start, end, price: 150m));
        create.EnsureSuccessStatusCode();
        var eventId = (await create.Content.ReadFromJsonAsync<ApiResponse<Guid>>(TestJson.Options))!.Data;

        var reservation = new CreateReservationCommand
        {
            EventId = eventId,
            Quantity = 11,
            PurchaserName = "Ana Pérez",
            PurchaserEmail = "ana@example.com"
        };

        var response = await _client.PostAsJsonAsync("/api/reservation", reservation);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        body.Should().Contain("entradas por transacción");
    }
}
