namespace Ceiba.LiveEvent.Reservations.Domain.Exceptions;

/// <summary>
/// Violación de una regla de negocio (RN). Lleva una <see cref="MessageKey"/> estable y,
/// opcionalmente, argumentos de formato; el texto a mostrar se resuelve en la capa de
/// presentación desde el catálogo de mensajes (manteniendo el dominio libre de textos de UI).
/// </summary>
public sealed class BusinessRuleException : DomainException
{
    public BusinessRuleException(string messageKey, params object[] args)
        : base(messageKey) // el mensaje base es la clave (fallback si no se resuelve)
    {
        MessageKey = messageKey;
        Args = args;
    }

    /// <summary>Clave del mensaje en el catálogo (p. ej. <c>rules.rn01VenueCapacity</c>).</summary>
    public string MessageKey { get; }

    /// <summary>Argumentos de formato para el mensaje.</summary>
    public object[] Args { get; }
}
