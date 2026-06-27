using System.Net;
using System.Net.Http.Json;
using Ceiba.LiveEvent.Reservations.Api.Common;
using Ceiba.LiveEvent.Reservations.Application.Events.Commands.CreateEvent;
using Ceiba.LiveEvent.Reservations.Application.Events.Dtos;
using Ceiba.LiveEvent.Reservations.Domain.Events;
using FluentAssertions;

namespace Ceiba.LiveEvent.Reservations.Api.IntegrationTests;

[Collection(nameof(DatabaseCollection))]
public class EventControllerTests
{
    private readonly HttpClient _client;

    public EventControllerTests(CustomWebApplicationFactory factory)
        => _client = factory.CreateClient();

    private static CreateEventCommand ValidCommand(string title = "Concierto de Integración")
    {
        var (start, end) = TestSchedule.NextSlot();
        return new()
        {
            Title = title,
            Description = "Descripción válida para la prueba de integración.",
            VenueId = CustomWebApplicationFactory.TestVenueId,
            MaximumCapacity = 100,
            StartDate = start,
            EndDate = end,
            TicketPrice = 80m,
            Type = EventType.Concierto
        };
    }

    [Fact]
    public async Task Create_ConDatosValidos_Devuelve201YPermiteRecuperarlo()
    {
        var response = await _client.PostAsJsonAsync("/api/event", ValidCommand());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>(TestJson.Options);
        created!.Message.Should().Be("El evento se creó correctamente.");
        var id = created.Data;

        var dto = await _client.GetFromJsonAsync<EventDto>($"/api/event/{id}", TestJson.Options);
        dto.Should().NotBeNull();
        // Estado inicial "activo" resuelto al id fijo del catálogo (seed 09_seed_event_status.sql).
        dto!.EventStatusId.Should().Be(new Guid("22222222-0000-0000-0000-000000000001"));
        // "Concierto" se resolvió al id fijo del catálogo (seed 11_seed_event_type.sql).
        dto.EventTypeId.Should().Be(new Guid("11111111-0000-0000-0000-000000000003"));
        dto.MaximumCapacity.Should().Be(100);
    }

    [Fact]
    public async Task Create_ConCapacidadMayorQueElVenue_Devuelve400()
    {
        var command = ValidCommand() with { MaximumCapacity = CustomWebApplicationFactory.TestVenueCapacity + 1 };

        var response = await _client.PostAsJsonAsync("/api/event", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_ConTituloVacio_Devuelve400()
    {
        var response = await _client.PostAsJsonAsync("/api/event", ValidCommand() with { Title = string.Empty });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAll_FiltraPorTituloParcial()
    {
        var unique = $"Filtrable-{Guid.NewGuid():N}";
        await _client.PostAsJsonAsync("/api/event", ValidCommand(unique));

        var events = await _client.GetFromJsonAsync<List<EventDto>>($"/api/event?title={unique[..15]}", TestJson.Options);

        events.Should().NotBeNull();
        events!.Should().ContainSingle(e => e.Title == unique);
    }

    [Fact]
    public async Task Cancel_PasaElEventoACancelado()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/event", ValidCommand());
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>(TestJson.Options);
        var id = created!.Data;

        var cancelResponse = await _client.PostAsync($"/api/event/{id}/cancel", null);
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await _client.GetFromJsonAsync<EventDto>($"/api/event/{id}", TestJson.Options);
        // "cancelado" resuelto al id fijo del catálogo (seed 09_seed_event_status.sql).
        dto!.EventStatusId.Should().Be(new Guid("22222222-0000-0000-0000-000000000002"));
    }

    [Fact]
    public async Task GetOccupancy_CuandoEventoNoExiste_Devuelve404()
    {
        var response = await _client.PostAsync($"/api/event/{Guid.NewGuid()}/occupancy", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CompleteFinished_DevuelveConteoYMensaje()
    {
        var response = await _client.PostAsync("/api/event/complete-finished", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<int>>(TestJson.Options);
        body.Should().NotBeNull();
        body!.Data.Should().BeGreaterThanOrEqualTo(0);
    }
}
