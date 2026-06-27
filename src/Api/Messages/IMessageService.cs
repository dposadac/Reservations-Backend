namespace Ceiba.LiveEvent.Reservations.Api.Messages;

/// <summary>
/// Acceso de solo lectura al catálogo de mensajes (Resources/messages.json),
/// localizado por clave con notación de puntos (p. ej. <c>"reservation.confirmed"</c>).
/// </summary>
public interface IMessageService
{
    /// <summary>Devuelve el mensaje asociado a la clave, o <c>[clave]</c> si no existe.</summary>
    string Get(string key);

    /// <summary>Devuelve el mensaje formateado con <see cref="string.Format(string, object?[])"/>.</summary>
    string Get(string key, params object?[] args);
}
