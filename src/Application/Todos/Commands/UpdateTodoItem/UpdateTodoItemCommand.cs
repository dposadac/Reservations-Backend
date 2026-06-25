using Ceiba.LiveEvent.Reservations.Domain.Todos.Enums;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Todos.Commands.UpdateTodoItem;

/// <summary>Comando para actualizar los detalles de una tarea existente.</summary>
public sealed record UpdateTodoItemCommand : IRequest
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public PriorityLevel Priority { get; init; } = PriorityLevel.None;
}
