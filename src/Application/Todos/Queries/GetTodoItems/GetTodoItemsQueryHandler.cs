using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Application.Todos.Dtos;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Todos.Queries.GetTodoItems;

public sealed class GetTodoItemsQueryHandler
    : IRequestHandler<GetTodoItemsQuery, IReadOnlyList<TodoItemDto>>
{
    private readonly ITodoRepository _repository;

    public GetTodoItemsQueryHandler(ITodoRepository repository)
        => _repository = repository;

    public async Task<IReadOnlyList<TodoItemDto>> Handle(
        GetTodoItemsQuery request,
        CancellationToken cancellationToken)
    {
        var items = await _repository.GetAllAsync(cancellationToken);

        return items
            .Where(i => request.OnlyPending != true || !i.IsCompleted)
            .OrderByDescending(i => i.Priority)
            .ThenByDescending(i => i.CreatedAt)
            .Select(TodoItemDto.FromEntity)
            .ToList();
    }
}
