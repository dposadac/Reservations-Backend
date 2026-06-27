using Ceiba.LiveEvent.Reservations.Domain.Common;
using Ceiba.LiveEvent.Reservations.Domain.Events.Events;
using Ceiba.LiveEvent.Reservations.Domain.Exceptions;

namespace Ceiba.LiveEvent.Reservations.Domain.Events;

/// <summary>
/// Evento que se celebra en un lugar. Raíz de agregado. Encapsula las invariantes
/// de creación (RF-01) y las transiciones de estado (RN-06).
/// </summary>
public class Event : BaseEntity, IAggregateRoot
{
    public const int TitleMinLength = 5;
    public const int TitleMaxLength = 100;
    public const int DescriptionMinLength = 10;
    public const int DescriptionMaxLength = 500;

    /// <summary>RN-03: hora límite de inicio en fin de semana (no puede iniciar después de las 22:00).</summary>
    public static readonly TimeSpan WeekendStartCutoff = TimeSpan.FromHours(22);

    // EF Core / serializadores necesitan un constructor sin parámetros.
    private Event()
    {
    }

    private Event(
        string title,
        string description,
        Guid venueId,
        int maximumCapacity,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        decimal ticketPrice,
        Guid eventTypeId,
        Guid eventStatusId)
    {
        Title = title;
        Description = description;
        VenueId = venueId;
        MaximumCapacity = maximumCapacity;
        StartDate = startDate;
        EndDate = endDate;
        TicketPrice = ticketPrice;
        EventTypeId = eventTypeId;
        EventStatusId = eventStatusId;
    }

    /// <summary>Título del evento (5-100 caracteres).</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Descripción del evento (10-500 caracteres).</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Lugar donde se celebra el evento.</summary>
    public Guid VenueId { get; private set; }

    /// <summary>Aforo máximo del evento (no puede superar la capacidad del lugar).</summary>
    public int MaximumCapacity { get; private set; }

    /// <summary>Fecha y hora de inicio.</summary>
    public DateTimeOffset StartDate { get; private set; }

    /// <summary>Fecha y hora de finalización.</summary>
    public DateTimeOffset EndDate { get; private set; }

    /// <summary>Precio de la entrada.</summary>
    public decimal TicketPrice { get; private set; }

    /// <summary>Tipo de evento (FK al catálogo <c>event_type</c>).</summary>
    public Guid EventTypeId { get; private set; }

    /// <summary>Estado del evento (FK al catálogo <c>event_status</c>, RN-06).</summary>
    public Guid EventStatusId { get; private set; }

    /// <summary>
    /// Crea un evento aplicando las invariantes de RF-01. El estado inicial es
    /// <see cref="EventStatus.Activo"/>.
    /// </summary>
    /// <param name="venueCapacity">Capacidad del lugar referenciado, para validar el aforo.</param>
    /// <param name="eventTypeId">Id del tipo de evento (resuelto contra el catálogo event_type).</param>
    /// <param name="eventStatusId">Id del estado inicial; por convención, el del estado "activo".</param>
    /// <param name="utcNow">Instante actual, para validar que la fecha de inicio sea futura.</param>
    public static Event Create(
        string title,
        string description,
        Guid venueId,
        int venueCapacity,
        int maximumCapacity,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        decimal ticketPrice,
        Guid eventTypeId,
        Guid eventStatusId,
        DateTimeOffset utcNow)
    {
        title = (title ?? string.Empty).Trim();
        description = (description ?? string.Empty).Trim();

        if (title.Length is < TitleMinLength or > TitleMaxLength)
        {
            throw new DomainException($"El título debe tener entre {TitleMinLength} y {TitleMaxLength} caracteres.");
        }

        if (description.Length is < DescriptionMinLength or > DescriptionMaxLength)
        {
            throw new DomainException(
                $"La descripción debe tener entre {DescriptionMinLength} y {DescriptionMaxLength} caracteres.");
        }

        if (venueId == Guid.Empty)
        {
            throw new DomainException("El lugar (venue) es obligatorio.");
        }

        if (maximumCapacity <= 0)
        {
            throw new DomainException("La capacidad máxima debe ser un entero positivo.");
        }

        // RN-01: la capacidad del evento no puede superar la capacidad del venue.
        if (maximumCapacity > venueCapacity)
        {
            throw new BusinessRuleException(RuleMessageKeys.Rn01VenueCapacity, maximumCapacity, venueCapacity);
        }

        if (startDate <= utcNow)
        {
            throw new DomainException("La fecha de inicio debe ser futura.");
        }

        if (endDate <= startDate)
        {
            throw new DomainException("La fecha de finalización debe ser posterior a la de inicio.");
        }

        // RN-03: en fin de semana (sábado/domingo) no puede iniciar después de las 22:00.
        var startsOnWeekend = startDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        if (startsOnWeekend && startDate.TimeOfDay > WeekendStartCutoff)
        {
            throw new BusinessRuleException(RuleMessageKeys.Rn03WeekendNight);
        }

        if (ticketPrice <= 0)
        {
            throw new DomainException("El precio de la entrada debe ser un decimal positivo.");
        }

        if (eventTypeId == Guid.Empty)
        {
            throw new DomainException("El tipo de evento es obligatorio.");
        }

        if (eventStatusId == Guid.Empty)
        {
            throw new DomainException("El estado del evento es obligatorio.");
        }

        var @event = new Event(
            title, description, venueId, maximumCapacity, startDate, endDate, ticketPrice, eventTypeId, eventStatusId);
        @event.AddDomainEvent(new EventCreatedEvent(@event));
        return @event;
    }

    /// <summary>
    /// Cancela el evento (RN-06). El id de "cancelado" lo resuelve el handler contra el catálogo.
    /// </summary>
    public void Cancel(Guid cancelledStatusId)
    {
        if (cancelledStatusId == Guid.Empty)
        {
            throw new DomainException("El estado 'cancelado' es obligatorio.");
        }

        if (EventStatusId == cancelledStatusId)
        {
            throw new DomainException("El evento ya está cancelado.");
        }

        EventStatusId = cancelledStatusId;
        AddDomainEvent(new EventCancelledEvent(this));
    }

    /// <summary>
    /// Pasa el evento a "completado" si está activo y ya finalizó (RN-06). Los ids de los
    /// estados los resuelve el handler contra el catálogo. Operación idempotente.
    /// </summary>
    public void RefreshStatus(Guid activeStatusId, Guid completedStatusId, DateTimeOffset utcNow)
    {
        if (EventStatusId == activeStatusId && utcNow >= EndDate)
        {
            EventStatusId = completedStatusId;
        }
    }
}
