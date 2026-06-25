using Ceiba.LiveEvent.Reservations.Domain.Common;

namespace Ceiba.LiveEvent.Reservations.Domain.Todos.Events;

/// <summary>Se dispara cuando una tarea se marca como completada.</summary>
public sealed class TodoItemCompletedEvent : BaseEvent
{
    public TodoItemCompletedEvent(TodoItem item) => Item = item;

    public TodoItem Item { get; }
}
