using Ceiba.LiveEvent.Reservations.Application.Todos.Dtos;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Todos.Queries.GetTodoItemById;

/// <summary>Consulta para obtener una tarea por su identificador.</summary>
public sealed record GetTodoItemByIdQuery(Guid Id) : IRequest<TodoItemDto>;
