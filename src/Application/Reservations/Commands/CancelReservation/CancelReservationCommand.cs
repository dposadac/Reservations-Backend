using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Reservations.Commands.CancelReservation;

/// <summary>
/// Comando para cancelar una reserva (RF-05). Devuelve <c>true</c> si la cancelación
/// penalizó (RN-07: menos de 48 horas para el evento, entradas marcadas como perdidas).
/// </summary>
public sealed record CancelReservationCommand(Guid ReservationId) : IRequest<bool>;
