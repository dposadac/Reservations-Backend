using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Reservations.Commands.CreateReservation;

/// <summary>Comando para reservar entradas de un evento (RF-03). Devuelve el id de la reserva.</summary>
public sealed record CreateReservationCommand : IRequest<Guid>
{
    public Guid EventId { get; init; }

    public int Quantity { get; init; }

    public string PurchaserName { get; init; } = string.Empty;

    public string PurchaserEmail { get; init; } = string.Empty;

    public string? City { get; init; }
}
