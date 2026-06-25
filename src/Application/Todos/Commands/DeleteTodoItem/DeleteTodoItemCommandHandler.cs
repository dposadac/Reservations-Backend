using Ceiba.LiveEvent.Reservations.Application.Common.Exceptions;
using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Domain.Todos;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Todos.Commands.DeleteTodoItem;

public sealed class DeleteTodoItemCommandHandler : IRequestHandler<DeleteTodoItemCommand>
{
    private readonly ITodoRepository _repository;

    public DeleteTodoItemCommandHandler(ITodoRepository repository)
        => _repository = repository;

    public async Task Handle(DeleteTodoItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(TodoItem), request.Id);

        _repository.Remove(item);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
