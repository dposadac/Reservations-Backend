using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Todos.Commands.DeleteTodoItem;

/// <summary>Comando para eliminar una tarea.</summary>
public sealed record DeleteTodoItemCommand(Guid Id) : IRequest;
