using Ceiba.LiveEvent.Reservations.Application.Events.Dtos;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Events.Queries.GetEventById;

/// <summary>Consulta para obtener un evento por su identificador.</summary>
public sealed record GetEventByIdQuery(Guid Id) : IRequest<EventDto>;
