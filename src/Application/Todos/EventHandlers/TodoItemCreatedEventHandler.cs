using Ceiba.LiveEvent.Reservations.Domain.Todos.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ceiba.LiveEvent.Reservations.Application.Todos.EventHandlers;

/// <summary>
/// Reacciona al evento de dominio de creación de una tarea. Ejemplo de cómo la capa
/// de aplicación maneja efectos secundarios sin acoplarlos a la lógica del agregado.
/// </summary>
public sealed class TodoItemCreatedEventHandler : INotificationHandler<TodoItemCreatedEvent>
{
    private readonly ILogger<TodoItemCreatedEventHandler> _logger;

    public TodoItemCreatedEventHandler(ILogger<TodoItemCreatedEventHandler> logger)
        => _logger = logger;

    public Task Handle(TodoItemCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Tarea creada: {TodoId} - {Title}",
            notification.Item.Id,
            notification.Item.Title);

        return Task.CompletedTask;
    }
}
