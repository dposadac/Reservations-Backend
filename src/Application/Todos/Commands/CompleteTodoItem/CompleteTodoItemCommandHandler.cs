using Ceiba.LiveEvent.Reservations.Application.Common.Exceptions;
using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Domain.Todos;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Todos.Commands.CompleteTodoItem;

public sealed class CompleteTodoItemCommandHandler : IRequestHandler<CompleteTodoItemCommand>
{
    private readonly ITodoRepository _repository;

    public CompleteTodoItemCommandHandler(ITodoRepository repository)
        => _repository = repository;

    public async Task Handle(CompleteTodoItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(TodoItem), request.Id);

        item.MarkAsComplete();

        _repository.Update(item);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
