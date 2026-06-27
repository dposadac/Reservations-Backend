using Ceiba.LiveEvent.Reservations.Domain.Reservations;

namespace Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;

/// <summary>Acceso de solo lectura al catálogo de estados de reserva (<c>reservation_status</c>).</summary>
public interface IReservationStatusRepository
{
    Task<IReadOnlyList<ReservationStatusLookup>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Busca un estado de reserva por su nombre (case-insensitive); null si no existe.</summary>
    Task<ReservationStatusLookup?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
