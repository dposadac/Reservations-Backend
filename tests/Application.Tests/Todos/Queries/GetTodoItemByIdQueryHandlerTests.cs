using Ceiba.LiveEvent.Reservations.Application.Common.Exceptions;
using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Application.Todos.Queries.GetTodoItemById;
using Ceiba.LiveEvent.Reservations.Domain.Todos;
using FluentAssertions;
using Moq;

namespace Ceiba.LiveEvent.Reservations.Application.Tests.Todos.Queries;

public class GetTodoItemByIdQueryHandlerTests
{
    private readonly Mock<ITodoRepository> _repository = new();

    [Fact]
    public async Task Handle_CuandoExiste_DevuelveElDto()
    {
        var item = TodoItem.Create("Tarea", "Descripción");
        _repository
            .Setup(r => r.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var handler = new GetTodoItemByIdQueryHandler(_repository.Object);

        var dto = await handler.Handle(new GetTodoItemByIdQuery(item.Id), CancellationToken.None);

        dto.Id.Should().Be(item.Id);
        dto.Title.Should().Be("Tarea");
        dto.Description.Should().Be("Descripción");
    }

    [Fact]
    public async Task Handle_CuandoNoExiste_LanzaNotFoundException()
    {
        _repository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TodoItem?)null);

        var handler = new GetTodoItemByIdQueryHandler(_repository.Object);

        var act = () => handler.Handle(new GetTodoItemByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
