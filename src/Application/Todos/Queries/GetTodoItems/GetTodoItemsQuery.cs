using Ceiba.LiveEvent.Reservations.Application.Todos.Dtos;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Todos.Queries.GetTodoItems;

/// <summary>Consulta para obtener el listado de tareas, con filtro opcional por estado.</summary>
public sealed record GetTodoItemsQuery(bool? OnlyPending = null) : IRequest<IReadOnlyList<TodoItemDto>>;
