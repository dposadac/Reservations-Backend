using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Application.Todos.Commands.CreateTodoItem;
using Ceiba.LiveEvent.Reservations.Domain.Todos;
using Ceiba.LiveEvent.Reservations.Domain.Todos.Enums;
using FluentAssertions;
using Moq;

namespace Ceiba.LiveEvent.Reservations.Application.Tests.Todos.Commands;

public class CreateTodoItemCommandHandlerTests
{
    private readonly Mock<ITodoRepository> _repository = new();

    [Fact]
    public async Task Handle_CreaLaTareaYDevuelveSuId()
    {
        TodoItem? added = null;
        _repository
            .Setup(r => r.AddAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()))
            .Callback<TodoItem, CancellationToken>((item, _) => added = item)
            .Returns(Task.CompletedTask);

        var handler = new CreateTodoItemCommandHandler(_repository.Object);
        var command = new CreateTodoItemCommand
        {
            Title = "Reservar sala",
            Description = "Sala principal",
            Priority = PriorityLevel.High
        };

        var id = await handler.Handle(command, CancellationToken.None);

        added.Should().NotBeNull();
        id.Should().Be(added!.Id);
        added.Title.Should().Be("Reservar sala");
        _repository.Verify(r => r.AddAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
