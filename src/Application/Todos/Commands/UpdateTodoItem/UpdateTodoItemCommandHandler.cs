using Ceiba.LiveEvent.Reservations.Application.Common.Exceptions;
using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Domain.Todos;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Todos.Commands.UpdateTodoItem;

public sealed class UpdateTodoItemCommandHandler : IRequestHandler<UpdateTodoItemCommand>
{
    private readonly ITodoRepository _repository;

    public UpdateTodoItemCommandHandler(ITodoRepository repository)
        => _repository = repository;

    public async Task Handle(UpdateTodoItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(TodoItem), request.Id);

        item.UpdateDetails(request.Title, request.Description, request.Priority);

        _repository.Update(item);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
