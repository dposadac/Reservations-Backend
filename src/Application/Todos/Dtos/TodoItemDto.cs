using Ceiba.LiveEvent.Reservations.Domain.Todos;
using Ceiba.LiveEvent.Reservations.Domain.Todos.Enums;

namespace Ceiba.LiveEvent.Reservations.Application.Todos.Dtos;

/// <summary>Proyección de lectura de una tarea para la capa de presentación.</summary>
public sealed record TodoItemDto
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public PriorityLevel Priority { get; init; }

    public bool IsCompleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? CompletedAt { get; init; }

    public static TodoItemDto FromEntity(TodoItem item) => new()
    {
        Id = item.Id,
        Title = item.Title,
        Description = item.Description,
        Priority = item.Priority,
        IsCompleted = item.IsCompleted,
        CreatedAt = item.CreatedAt,
        CompletedAt = item.CompletedAt
    };
}
