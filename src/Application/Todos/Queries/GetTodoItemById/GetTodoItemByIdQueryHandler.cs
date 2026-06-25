using Ceiba.LiveEvent.Reservations.Application.Common.Exceptions;
using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Application.Todos.Dtos;
using Ceiba.LiveEvent.Reservations.Domain.Todos;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Todos.Queries.GetTodoItemById;

public sealed class GetTodoItemByIdQueryHandler
    : IRequestHandler<GetTodoItemByIdQuery, TodoItemDto>
{
    private readonly ITodoRepository _repository;

    public GetTodoItemByIdQueryHandler(ITodoRepository repository)
        => _repository = repository;

    public async Task<TodoItemDto> Handle(
        GetTodoItemByIdQuery request,
        CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(TodoItem), request.Id);

        return TodoItemDto.FromEntity(item);
    }
}
