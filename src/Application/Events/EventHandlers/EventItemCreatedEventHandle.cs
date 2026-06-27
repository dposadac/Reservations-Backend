using Ceiba.LiveEvent.Reservations.Domain.Events.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ceiba.LiveEvent.Reservations.Application.Events.EventHandlers;

/// <summary>Reacciona al evento de dominio de creación de un evento (RF-01).</summary>
public sealed class EventCreatedEventHandler : INotificationHandler<EventCreatedEvent>
{
    private readonly ILogger<EventCreatedEventHandler> _logger;

    public EventCreatedEventHandler(ILogger<EventCreatedEventHandler> logger)
        => _logger = logger;

    public Task Handle(EventCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Evento creado: {EventId} - {Title}",
            notification.Event.Id,
            notification.Event.Title);

        return Task.CompletedTask;
    }
}
