using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Events.Commands.CompleteFinishedEvents;

/// <summary>
/// Marca como "completado" todos los eventos activos cuya fecha de fin ya pasó (RN-06).
/// Devuelve la cantidad de eventos actualizados.
/// </summary>
public sealed record CompleteFinishedEventsCommand : IRequest<int>;
