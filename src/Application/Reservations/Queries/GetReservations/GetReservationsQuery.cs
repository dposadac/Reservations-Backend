using Ceiba.LiveEvent.Reservations.Application.Reservations.Dtos;
using Ceiba.LiveEvent.Reservations.Domain.Reservations;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Reservations.Queries.GetReservations;

/// <summary>Consulta para listar reservas con filtros opcionales.</summary>
public sealed record GetReservationsQuery : IRequest<IReadOnlyList<ReservationDto>>
{
    /// <summary>Filtra por evento.</summary>
    public Guid? EventId { get; init; }

    /// <summary>Filtra por estado de la reserva.</summary>
    public ReservationStatus? Status { get; init; }
}
