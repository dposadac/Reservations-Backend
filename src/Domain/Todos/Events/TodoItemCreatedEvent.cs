using Ceiba.LiveEvent.Reservations.Domain.Common;

namespace Ceiba.LiveEvent.Reservations.Domain.Todos.Events;

/// <summary>Se dispara cuando se crea una nueva tarea.</summary>
public sealed class TodoItemCreatedEvent : BaseEvent
{
    public TodoItemCreatedEvent(TodoItem item) => Item = item;

    public TodoItem Item { get; }
}
