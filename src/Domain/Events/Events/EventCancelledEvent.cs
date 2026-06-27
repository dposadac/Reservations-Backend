using Ceiba.LiveEvent.Reservations.Domain.Common;

namespace Ceiba.LiveEvent.Reservations.Domain.Events.Events;

/// <summary>Se dispara cuando un evento se cancela (RN-06).</summary>
public sealed class EventCancelledEvent : BaseEvent
{
    public EventCancelledEvent(Event @event) => Event = @event;

    public Event Event { get; }
}
