using System.Net;
using System.Net.Http.Json;
using Ceiba.LiveEvent.Reservations.Application.Todos.Commands.CreateTodoItem;
using Ceiba.LiveEvent.Reservations.Application.Todos.Dtos;
using Ceiba.LiveEvent.Reservations.Domain.Todos.Enums;
using FluentAssertions;

namespace Ceiba.LiveEvent.Reservations.Api.IntegrationTests;

[Collection(nameof(DatabaseCollection))]
public class TodoItemsControllerTests
{
    private readonly HttpClient _client;

    public TodoItemsControllerTests(CustomWebApplicationFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task Create_ConDatosValidos_Devuelve201YPermiteRecuperarla()
    {
        var command = new CreateTodoItemCommand
        {
            Title = "Reservar catering",
            Description = "Menú para 200 personas",
            Priority = PriorityLevel.High
        };

        var createResponse = await _client.PostAsJsonAsync("/api/todoitems", command);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();
        id.Should().NotBe(Guid.Empty);

        var getResponse = await _client.GetAsync($"/api/todoitems/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await getResponse.Content.ReadFromJsonAsync<TodoItemDto>();
        dto.Should().NotBeNull();
        dto!.Title.Should().Be("Reservar catering");
        dto.Priority.Should().Be(PriorityLevel.High);
        dto.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task Complete_MarcaLaTareaComoCompletada()
    {
        var id = await CreateTodoAsync("Tarea a completar");

        var completeResponse = await _client.PostAsync($"/api/todoitems/{id}/complete", null);
        completeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dto = await _client.GetFromJsonAsync<TodoItemDto>($"/api/todoitems/{id}");
        dto!.IsCompleted.Should().BeTrue();
        dto.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Delete_EliminaLaTarea()
    {
        var id = await CreateTodoAsync("Tarea a eliminar");

        var deleteResponse = await _client.DeleteAsync($"/api/todoitems/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/todoitems/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ConTituloVacio_Devuelve400()
    {
        var command = new CreateTodoItemCommand { Title = string.Empty };

        var response = await _client.PostAsJsonAsync("/api/todoitems", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_Devuelve404()
    {
        var response = await _client.GetAsync($"/api/todoitems/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_DevuelveLasTareasCreadas()
    {
        await CreateTodoAsync("Tarea listada");

        var items = await _client.GetFromJsonAsync<List<TodoItemDto>>("/api/todoitems");

        items.Should().NotBeNull();
        items!.Should().Contain(i => i.Title == "Tarea listada");
    }

    private async Task<Guid> CreateTodoAsync(string title)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/todoitems",
            new CreateTodoItemCommand { Title = title });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>();
    }
}
