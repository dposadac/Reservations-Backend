using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Domain.Todos;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Todos.Commands.CreateTodoItem;

public sealed class CreateTodoItemCommandHandler : IRequestHandler<CreateTodoItemCommand, Guid>
{
    private readonly ITodoRepository _repository;

    public CreateTodoItemCommandHandler(ITodoRepository repository)
        => _repository = repository;

    public async Task<Guid> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
    {
        var item = TodoItem.Create(request.Title, request.Description, request.Priority);

        await _repository.AddAsync(item, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return item.Id;
    }
}
