using Ceiba.LiveEvent.Reservations.Domain.Venues;

namespace Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;

/// <summary>
/// Abstracción de persistencia para la raíz de agregado <see cref="Venue"/>.
/// Definida en la capa de aplicación e implementada en infraestructura (DIP).
/// </summary>
public interface IVenueRepository
{
    Task<Venue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Venue>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Venue item, CancellationToken cancellationToken = default);

    void Update(Venue item);

    void Remove(Venue item);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
