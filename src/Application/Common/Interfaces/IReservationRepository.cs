using Ceiba.LiveEvent.Reservations.Domain.Reservations;

namespace Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;

/// <summary>
/// Abstracción de persistencia para la raíz de agregado <see cref="Reservation"/>.
/// Definida en la capa de aplicación e implementada en infraestructura (DIP).
/// </summary>
public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Reservation>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Todas las reservas de un evento (para validar aforo y construir reportes).</summary>
    Task<IReadOnlyList<Reservation>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default);

    /// <summary>Indica si ya existe una reserva con el código indicado (unicidad de RF-04).</summary>
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task AddAsync(Reservation item, CancellationToken cancellationToken = default);

    void Update(Reservation item);

    void Remove(Reservation item);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
