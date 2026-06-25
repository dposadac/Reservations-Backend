using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Todos.Commands.CompleteTodoItem;

/// <summary>Comando para marcar una tarea como completada.</summary>
public sealed record CompleteTodoItemCommand(Guid Id) : IRequest;
