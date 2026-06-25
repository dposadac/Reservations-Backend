using Ceiba.LiveEvent.Reservations.Domain.Todos.Enums;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Todos.Commands.CreateTodoItem;

/// <summary>Comando para crear una nueva tarea. Devuelve el identificador generado.</summary>
public sealed record CreateTodoItemCommand : IRequest<Guid>
{
    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public PriorityLevel Priority { get; init; } = PriorityLevel.None;
}
