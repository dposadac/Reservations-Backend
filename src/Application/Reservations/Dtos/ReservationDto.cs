using Ceiba.LiveEvent.Reservations.Domain.Reservations;

namespace Ceiba.LiveEvent.Reservations.Application.Reservations.Dtos;

/// <summary>Proyección de lectura de una reserva para la capa de presentación.</summary>
public sealed record ReservationDto
{
    public Guid Id { get; init; }

    public Guid EventId { get; init; }

    public Guid ReservationStatusId { get; init; }

    public string ReservationStatusName { get; init; } = string.Empty;

    public int Quantity { get; init; }

    public string PurchaserName { get; init; } = string.Empty;

    public string PurchaserEmail { get; init; } = string.Empty;

    public string? City { get; init; }

    public string? Code { get; init; }

    public DateTimeOffset? CancelledAt { get; init; }

    public bool IsForfeited { get; init; }

    public static ReservationDto FromEntity(Reservation reservation, string reservationStatusName = "") => new()
    {
        Id = reservation.Id,
        EventId = reservation.EventId,
        ReservationStatusId = reservation.ReservationStatusId,
        ReservationStatusName = reservationStatusName,
        Quantity = reservation.Quantity,
        PurchaserName = reservation.PurchaserName,
        PurchaserEmail = reservation.PurchaserEmail.Value,
        City = reservation.City,
        Code = reservation.Code?.Value,
        CancelledAt = reservation.CancelledAt,
        IsForfeited = reservation.IsForfeited
    };
}
