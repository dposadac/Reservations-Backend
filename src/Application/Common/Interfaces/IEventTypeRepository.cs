using Ceiba.LiveEvent.Reservations.Domain.Events;

namespace Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;

/// <summary>
/// Acceso de solo lectura al catálogo de tipos de evento (<c>event_type</c>), para
/// resolver un tipo por su nombre (texto).
/// </summary>
public interface IEventTypeRepository
{
    /// <summary>Busca un tipo de evento por su nombre (case-insensitive); null si no existe.</summary>
    Task<EventTypeLookup?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Devuelve todos los tipos de evento del catálogo.</summary>
    Task<IReadOnlyList<EventTypeLookup>> GetAllAsync(CancellationToken cancellationToken = default);
}
