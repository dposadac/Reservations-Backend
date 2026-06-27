using Ceiba.LiveEvent.Reservations.Domain.Common;

namespace Ceiba.LiveEvent.Reservations.Domain.Events.Events;

/// <summary>Se dispara cuando se crea un nuevo evento (RF-01).</summary>
public sealed class EventCreatedEvent : BaseEvent
{
    public EventCreatedEvent(Event @event) => Event = @event;

    public Event Event { get; }
}
