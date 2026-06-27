using Ceiba.LiveEvent.Reservations.Domain.Common;
using Ceiba.LiveEvent.Reservations.Domain.Exceptions;

namespace Ceiba.LiveEvent.Reservations.Domain.Events;

/// <summary>
/// Reglas de programación de eventos sobre un mismo venue. Servicio de dominio sin estado;
/// el llamador (handler) aporta los eventos activos del venue ya cargados.
/// </summary>
public static class EventScheduling
{
    /// <summary>
    /// RN-02: valida que el horario [start, end) no se superponga con ningún evento activo
    /// del mismo venue. Dos intervalos se superponen si <c>start &lt; otroFin</c> y
    /// <c>otroInicio &lt; end</c>.
    /// </summary>
    /// <param name="activeVenueEvents">Eventos activos del mismo venue (excluido el candidato).</param>
    public static void EnsureNoVenueOverlap(
        DateTimeOffset start,
        DateTimeOffset end,
        IEnumerable<Event> activeVenueEvents)
    {
        var overlaps = activeVenueEvents.Any(e => start < e.EndDate && e.StartDate < end);
        if (overlaps)
        {
            throw new BusinessRuleException(RuleMessageKeys.Rn02VenueOverlap);
        }
    }
}
