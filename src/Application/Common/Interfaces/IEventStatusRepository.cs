using Ceiba.LiveEvent.Reservations.Domain.Events;

namespace Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;

/// <summary>
/// Acceso de solo lectura al catálogo de estados de evento (<c>event_status</c>), para
/// resolver un estado por su nombre (texto).
/// </summary>
public interface IEventStatusRepository
{
    /// <summary>Busca un estado de evento por su nombre (case-insensitive); null si no existe.</summary>
    Task<EventStatusLookup?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Devuelve todos los estados de evento del catálogo.</summary>
    Task<IReadOnlyList<EventStatusLookup>> GetAllAsync(CancellationToken cancellationToken = default);
}
