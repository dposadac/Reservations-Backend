namespace Ceiba.LiveEvent.Reservations.Domain.Common;

/// <summary>
/// Claves de los mensajes de validación de las reglas de negocio (RN). El texto vive en
/// el catálogo de mensajes de la capa API (<c>Resources/messages.json</c>, sección <c>rules</c>).
/// </summary>
public static class RuleMessageKeys
{
    /// <summary>RN-01: la capacidad del evento no puede superar la del venue.</summary>
    public const string Rn01VenueCapacity = "rules.rn01VenueCapacity";

    /// <summary>RN-02: dos eventos activos no pueden compartir venue con horarios superpuestos.</summary>
    public const string Rn02VenueOverlap = "rules.rn02VenueOverlap";

    /// <summary>RN-03: en fin de semana un evento no puede iniciar después de las 22:00.</summary>
    public const string Rn03WeekendNight = "rules.rn03WeekendNight";

    /// <summary>RN-04: no se permiten reservas para eventos que inician en menos de 1 hora.</summary>
    public const string Rn04LateReservation = "rules.rn04LateReservation";

    /// <summary>RN-05: eventos con precio &gt; $100 limitan a 10 entradas por transacción.</summary>
    public const string Rn05PriceTicketLimit = "rules.rn05PriceTicketLimit";
}
