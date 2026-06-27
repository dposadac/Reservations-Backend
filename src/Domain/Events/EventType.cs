namespace Ceiba.LiveEvent.Reservations.Domain.Events;

/// <summary>
/// Tipo de evento. Persiste como FK a la tabla maestra <c>event_type</c>
/// mediante UUID fijos (ver convertidor en infraestructura).
/// </summary>
public enum EventType
{
    Conferencia = 1,
    Taller = 2,
    Concierto = 3
}
