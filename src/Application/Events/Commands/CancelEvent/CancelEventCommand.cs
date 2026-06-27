using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Events.Commands.CancelEvent;

/// <summary>Comando para cancelar un evento (RN-06).</summary>
public sealed record CancelEventCommand(Guid Id) : IRequest;
