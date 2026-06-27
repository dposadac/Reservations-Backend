using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Reservations.Commands.ConfirmReservationPayment;

/// <summary>Comando para confirmar el pago de una reserva (RF-04). Devuelve el código generado.</summary>
public sealed record ConfirmReservationPaymentCommand(Guid ReservationId) : IRequest<string>;
