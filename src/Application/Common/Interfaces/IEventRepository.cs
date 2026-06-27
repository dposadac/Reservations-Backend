using Ceiba.LiveEvent.Reservations.Domain.Events;

namespace Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;

/// <summary>
/// Abstracción de persistencia para la raíz de agregado <see cref="Event"/>.
/// Definida en la capa de aplicación e implementada en infraestructura (DIP).
/// </summary>
public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Eventos de un venue (para validar superposición de horarios, RN-02).</summary>
    Task<IReadOnlyList<Event>> GetByVenueAsync(Guid venueId, CancellationToken cancellationToken = default);

    Task AddAsync(Event item, CancellationToken cancellationToken = default);

    void Update(Event item);

    void Remove(Event item);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
