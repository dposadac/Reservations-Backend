using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Ceiba.LiveEvent.Reservations.Api.Common;
using Ceiba.LiveEvent.Reservations.Application.Events.Commands.CreateEvent;
using Ceiba.LiveEvent.Reservations.Application.Events.Dtos;
using Ceiba.LiveEvent.Reservations.Application.Reservations.Commands.CreateReservation;
using Ceiba.LiveEvent.Reservations.Domain.Events;
using FluentAssertions;

namespace Ceiba.LiveEvent.Reservations.Api.IntegrationTests;

[Collection(nameof(DatabaseCollection))]
public class ReservationControllerTests
{
    private readonly HttpClient _client;

    public ReservationControllerTests(CustomWebApplicationFactory factory)
        => _client = factory.CreateClient();

    private async Task<Guid> CreateEventAsync(int capacity = 100, decimal price = 50m)
    {
        var (start, end) = TestSchedule.NextSlot();
        var command = new CreateEventCommand
        {
            Title = "Evento para reservas",
            Description = "Descripción válida para la prueba de integración.",
            VenueId = CustomWebApplicationFactory.TestVenueId,
            MaximumCapacity = capacity,
            StartDate = start,
            EndDate = end,
            TicketPrice = price,
            Type = EventType.Conferencia
        };

        var response = await _client.PostAsJsonAsync("/api/event", command);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>(TestJson.Options);
        return body!.Data;
    }

    private async Task<Guid> CreateReservationAsync(Guid eventId, int quantity = 2)
    {
        var command = new CreateReservationCommand
        {
            EventId = eventId,
            Quantity = quantity,
            PurchaserName = "Ana Pérez",
            PurchaserEmail = "ana@example.com"
        };

        var response = await _client.PostAsJsonAsync("/api/reservation", command);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>(TestJson.Options);
        return body!.Data;
    }

    [Fact]
    public async Task Reserve_Confirm_Occupancy_FlujoCompleto()
    {
        var eventId = await CreateEventAsync(capacity: 100, price: 50m);
        var reservationId = await CreateReservationAsync(eventId, quantity: 4);

        // RF-04: confirmar pago devuelve un código EV-###### dentro de la envoltura.
        var confirmResponse = await _client.PostAsync($"/api/reservation/{reservationId}/confirm", null);
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var confirm = await confirmResponse.Content.ReadFromJsonAsync<ApiResponse<string>>(TestJson.Options);
        Regex.IsMatch(confirm!.Data!, @"^EV-\d{6}$").Should().BeTrue();
        confirm.Message.Should().Contain(confirm.Data!);

        // RF-06: el reporte de ocupación (POST) refleja las entradas vendidas y los ingresos.
        var occupancyResponse = await _client.PostAsync($"/api/event/{eventId}/occupancy", null);
        occupancyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var occupancy = await occupancyResponse.Content.ReadFromJsonAsync<OccupancyReportDto>(TestJson.Options);
        occupancy.Should().NotBeNull();
        occupancy!.TicketsSold.Should().Be(4);
        occupancy.AvailableTickets.Should().Be(96);
        occupancy.TotalRevenue.Should().Be(200m);
    }

    [Fact]
    public async Task Confirm_DosVeces_Devuelve400()
    {
        var eventId = await CreateEventAsync();
        var reservationId = await CreateReservationAsync(eventId);

        await _client.PostAsync($"/api/reservation/{reservationId}/confirm", null);
        var second = await _client.PostAsync($"/api/reservation/{reservationId}/confirm", null);

        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Cancel_ReservaConfirmada_Devuelve200ConMensaje()
    {
        var eventId = await CreateEventAsync();
        var reservationId = await CreateReservationAsync(eventId);
        await _client.PostAsync($"/api/reservation/{reservationId}/confirm", null);

        var cancelResponse = await _client.PostAsync($"/api/reservation/{reservationId}/cancel", null);

        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await cancelResponse.Content.ReadFromJsonAsync<ApiResponse<object>>(TestJson.Options);
        body!.Message.Should().Be("La reserva se canceló correctamente.");
    }

    [Fact]
    public async Task Cancel_ReservaPendiente_Devuelve400()
    {
        var eventId = await CreateEventAsync();
        var reservationId = await CreateReservationAsync(eventId);

        var cancelResponse = await _client.PostAsync($"/api/reservation/{reservationId}/cancel", null);

        cancelResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Reserve_ConEmailInvalido_Devuelve400()
    {
        var eventId = await CreateEventAsync();
        var command = new CreateReservationCommand
        {
            EventId = eventId,
            Quantity = 1,
            PurchaserName = "Ana",
            PurchaserEmail = "no-es-email"
        };

        var response = await _client.PostAsJsonAsync("/api/reservation", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
