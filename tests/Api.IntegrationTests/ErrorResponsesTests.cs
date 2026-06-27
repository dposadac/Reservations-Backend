using System.Net;
using System.Net.Http.Json;
using Ceiba.LiveEvent.Reservations.Application.Events.Commands.CreateEvent;
using Ceiba.LiveEvent.Reservations.Domain.Events;
using FluentAssertions;

namespace Ceiba.LiveEvent.Reservations.Api.IntegrationTests;

/// <summary>
/// Verifica que las respuestas de error muestran los mensajes esperados: los del
/// catálogo (títulos) y los de FluentValidation / dominio (detalle por campo o regla).
/// </summary>
[Collection(nameof(DatabaseCollection))]
public class ErrorResponsesTests
{
    private readonly HttpClient _client;

    public ErrorResponsesTests(CustomWebApplicationFactory factory)
        => _client = factory.CreateClient();

    private static CreateEventCommand ValidCommand() => new()
    {
        Title = "Evento válido",
        Description = "Descripción válida para la prueba.",
        VenueId = CustomWebApplicationFactory.TestVenueId,
        MaximumCapacity = 50,
        StartDate = DateTimeOffset.UtcNow.AddDays(10),
        EndDate = DateTimeOffset.UtcNow.AddDays(10).AddHours(2),
        TicketPrice = 25m,
        Type = EventType.Taller
    };

    [Fact]
    public async Task Validacion_DevuelveTituloDelCatalogoYMensajesPorCampo()
    {
        var command = ValidCommand() with { Title = string.Empty, TicketPrice = 0m };

        var response = await _client.PostAsJsonAsync("/api/event", command);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        // Título del catálogo (error.validation).
        body.Should().Contain("Se han producido uno o más errores de validación");
        // Mensajes de FluentValidation por campo.
        body.Should().Contain("título");
        body.Should().Contain("precio");
    }

    [Fact]
    public async Task ReglaDeNegocio_DevuelveTituloDelCatalogoYDetalleDelDominio()
    {
        var command = ValidCommand() with { MaximumCapacity = CustomWebApplicationFactory.TestVenueCapacity + 1 };

        var response = await _client.PostAsJsonAsync("/api/event", command);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        body.Should().Contain("regla de negocio");          // catálogo (error.businessRule)
        body.Should().Contain("capacidad del lugar");       // detalle de la DomainException
    }

    [Fact]
    public async Task NoEncontrado_DevuelveTituloDelCatalogo()
    {
        var response = await _client.PostAsync($"/api/event/{Guid.NewGuid()}/occupancy", null);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        body.Should().Contain("recurso solicitado");        // catálogo (error.notFound)
    }
}
