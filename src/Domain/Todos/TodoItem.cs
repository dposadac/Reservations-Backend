using Ceiba.LiveEvent.Reservations.Domain.Common;
using Ceiba.LiveEvent.Reservations.Domain.Exceptions;
using Ceiba.LiveEvent.Reservations.Domain.Todos.Enums;
using Ceiba.LiveEvent.Reservations.Domain.Todos.Events;

namespace Ceiba.LiveEvent.Reservations.Domain.Todos;

/// <summary>
/// Raíz de agregado que representa una tarea (TODO). Encapsula sus invariantes y
/// expone comportamiento en lugar de permitir mutaciones arbitrarias de su estado.
/// </summary>
public class TodoItem : BaseAuditableEntity, IAggregateRoot
{
    // EF Core / serializadores necesitan un constructor sin parámetros.
    private TodoItem()
    {
    }

    private TodoItem(string title, string? description, PriorityLevel priority)
    {
        Title = title;
        Description = description;
        Priority = priority;
        IsCompleted = false;
    }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public PriorityLevel Priority { get; private set; }

    public bool IsCompleted { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    /// <summary>Crea una nueva tarea aplicando las invariantes del dominio.</summary>
    public static TodoItem Create(string title, string? description = null, PriorityLevel priority = PriorityLevel.None)
    {
        EnsureValidTitle(title);

        var item = new TodoItem(title.Trim(), description?.Trim(), priority);
        item.AddDomainEvent(new TodoItemCreatedEvent(item));
        return item;
    }

    /// <summary>Actualiza los detalles editables de la tarea.</summary>
    public void UpdateDetails(string title, string? description, PriorityLevel priority)
    {
        EnsureValidTitle(title);

        Title = title.Trim();
        Description = description?.Trim();
        Priority = priority;
    }

    /// <summary>Marca la tarea como completada. Operación idempotente.</summary>
    public void MarkAsComplete()
    {
        if (IsCompleted)
        {
            return;
        }

        IsCompleted = true;
        CompletedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new TodoItemCompletedEvent(this));
    }

    /// <summary>Reabre una tarea previamente completada.</summary>
    public void Reopen()
    {
        IsCompleted = false;
        CompletedAt = null;
    }

    private static void EnsureValidTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("El título de la tarea es obligatorio.");
        }

        if (title.Trim().Length > 200)
        {
            throw new DomainException("El título de la tarea no puede superar los 200 caracteres.");
        }
    }
}
