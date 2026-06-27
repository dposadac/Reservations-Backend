namespace Ceiba.LiveEvent.Reservations.Domain.Events;

/// <summary>
/// Estado de un evento (RN-06). Persiste como FK a la tabla maestra
/// <c>event_status</c> mediante UUID fijos (ver convertidor en infraestructura).
/// </summary>
public enum EventStatus
{
    Activo = 1,
    Cancelado = 2,
    Completado = 3
}
