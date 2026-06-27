using System.Net;
using System.Net.Http.Json;
using Ceiba.LiveEvent.Reservations.Application.Masters.Dtos;
using FluentAssertions;

namespace Ceiba.LiveEvent.Reservations.Api.IntegrationTests;

[Collection(nameof(DatabaseCollection))]
public class MasterControllerTests
{
    private readonly HttpClient _client;

    public MasterControllerTests(CustomWebApplicationFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task GetAll_DevuelveTodosLosMaestros()
    {
        var masters = await _client.GetFromJsonAsync<MastersDto>("/api/master", TestJson.Options);

        masters.Should().NotBeNull();
        masters!.TipoEventos.Should().HaveCount(3).And.Contain(t => t.Name == "conferencia");
        masters.EventosEstados.Should().HaveCount(3).And.Contain(s => s.Name == "activo");
        masters.ReservasEstados.Should().HaveCount(3).And.Contain(s => s.Name == "pendiente_pago");
        masters.Venues.Should().Contain(v => v.Id == CustomWebApplicationFactory.TestVenueId);
    }

    [Fact]
    public async Task GetAll_RespondeConLaFormaJsonEsperada()
    {
        var response = await _client.GetAsync("/api/master");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().Contain("\"tipoEventos\"");
        body.Should().Contain("\"eventosEstados\"");
        body.Should().Contain("\"reservasestados\"");
        body.Should().Contain("\"venues\"");
    }
}
