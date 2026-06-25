using Ceiba.LiveEvent.Reservations.Application.Common.Exceptions;
using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Application.Todos.Commands.CompleteTodoItem;
using Ceiba.LiveEvent.Reservations.Domain.Todos;
using FluentAssertions;
using Moq;

namespace Ceiba.LiveEvent.Reservations.Application.Tests.Todos.Commands;

public class CompleteTodoItemCommandHandlerTests
{
    private readonly Mock<ITodoRepository> _repository = new();

    [Fact]
    public async Task Handle_CuandoExiste_MarcaLaTareaComoCompletada()
    {
        var item = TodoItem.Create("Tarea");
        _repository
            .Setup(r => r.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var handler = new CompleteTodoItemCommandHandler(_repository.Object);

        await handler.Handle(new CompleteTodoItemCommand(item.Id), CancellationToken.None);

        item.IsCompleted.Should().BeTrue();
        _repository.Verify(r => r.Update(item), Times.Once);
        _repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CuandoNoExiste_LanzaNotFoundException()
    {
        _repository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TodoItem?)null);

        var handler = new CompleteTodoItemCommandHandler(_repository.Object);

        var act = () => handler.Handle(new CompleteTodoItemCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
