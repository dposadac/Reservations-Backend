using Ceiba.LiveEvent.Reservations.Domain.Common;
using Ceiba.LiveEvent.Reservations.Domain.Exceptions;
using Ceiba.LiveEvent.Reservations.Domain.Reservations.Events;

namespace Ceiba.LiveEvent.Reservations.Domain.Reservations;

/// <summary>
/// Reserva de entradas para un evento. Raíz de agregado. Encapsula las reglas de
/// creación (RF-03), confirmación de pago (RF-04) y cancelación (RF-05). El estado se
/// guarda como FK al catálogo <c>reservation_status</c>; los ids los resuelven los handlers.
/// </summary>
public class Reservation : BaseEntity, IAggregateRoot
{
    /// <summary>Ventana, en horas, dentro de la cual aplica el límite reducido de entradas (RF-03).</summary>
    public const int LastMinuteWindowHours = 24;

    /// <summary>Máximo de entradas por transacción dentro de la ventana de última hora (RF-03).</summary>
    public const int LastMinuteMaxQuantity = 5;

    /// <summary>RN-04: no se permiten reservas si faltan menos de estas horas para el inicio.</summary>
    public const int MinimumHoursBeforeStart = 1;

    /// <summary>RN-05: umbral de precio por encima del cual se limitan las entradas por transacción.</summary>
    public const decimal HighPriceThreshold = 100m;

    /// <summary>RN-05: máximo de entradas por transacción para eventos de precio alto.</summary>
    public const int HighPriceMaxQuantity = 10;

    /// <summary>RN-07: ventana, en horas, dentro de la cual la cancelación penaliza (entradas perdidas).</summary>
    public const int PenaltyWindowHours = 48;

    // EF Core / serializadores necesitan un constructor sin parámetros.
    private Reservation()
    {
    }

    private Reservation(
        Guid eventId,
        int quantity,
        string purchaserName,
        Email purchaserEmail,
        string? city,
        Guid reservationStatusId)
    {
        EventId = eventId;
        Quantity = quantity;
        PurchaserName = purchaserName;
        PurchaserEmail = purchaserEmail;
        City = city;
        ReservationStatusId = reservationStatusId;
    }

    /// <summary>Evento reservado.</summary>
    public Guid EventId { get; private set; }

    /// <summary>Estado de la reserva (FK al catálogo <c>reservation_status</c>).</summary>
    public Guid ReservationStatusId { get; private set; }

    /// <summary>Cantidad de entradas reservadas.</summary>
    public int Quantity { get; private set; }

    /// <summary>Nombre del comprador.</summary>
    public string PurchaserName { get; private set; } = string.Empty;

    /// <summary>Correo del comprador.</summary>
    public Email PurchaserEmail { get; private set; } = null!;

    /// <summary>Ciudad del comprador.</summary>
    public string? City { get; private set; }

    /// <summary>Código único de reserva, asignado al confirmar el pago (RF-04).</summary>
    public ReservationCode? Code { get; private set; }

    /// <summary>Fecha y hora de cancelación (RF-05).</summary>
    public DateTimeOffset? CancelledAt { get; private set; }

    /// <summary>
    /// Indica que las entradas se perdieron por penalización (RN-07): la cancelación
    /// no las libera para la venta.
    /// </summary>
    public bool IsForfeited { get; private set; }

    /// <summary>
    /// Crea una reserva en estado "pendiente de pago" aplicando RF-03.
    /// </summary>
    /// <param name="availableTickets">Entradas disponibles del evento (capacidad menos ocupadas y perdidas).</param>
    /// <param name="eventStartDate">Inicio del evento, para la regla de última hora.</param>
    /// <param name="ticketPrice">Precio de la entrada del evento (RN-05).</param>
    /// <param name="utcNow">Instante actual.</param>
    /// <param name="reservationStatusId">Id del estado inicial; por convención, el de "pendiente_pago".</param>
    public static Reservation Create(
        Guid eventId,
        int quantity,
        string purchaserName,
        string purchaserEmail,
        int availableTickets,
        DateTimeOffset eventStartDate,
        decimal ticketPrice,
        DateTimeOffset utcNow,
        Guid reservationStatusId,
        string? city = null)
    {
        if (eventId == Guid.Empty)
        {
            throw new DomainException("El evento es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(purchaserName))
        {
            throw new DomainException("El nombre del comprador es obligatorio.");
        }

        if (quantity < 1)
        {
            throw new DomainException("La cantidad de entradas debe ser 1 o más.");
        }

        if (reservationStatusId == Guid.Empty)
        {
            throw new DomainException("El estado de la reserva es obligatorio.");
        }

        // Valida el formato del correo (lanza DomainException si es inválido).
        var email = Email.Create(purchaserEmail);

        if (quantity > availableTickets)
        {
            throw new DomainException(
                $"No hay entradas disponibles suficientes. Disponibles: {Math.Max(availableTickets, 0)}, solicitadas: {quantity}.");
        }

        var hoursUntilStart = (eventStartDate - utcNow).TotalHours;

        // RN-04: no se permiten reservas para eventos que inician en menos de 1 hora.
        if (hoursUntilStart < MinimumHoursBeforeStart)
        {
            throw new BusinessRuleException(RuleMessageKeys.Rn04LateReservation);
        }

        // RF-03: a menos de 24 horas del inicio solo se permiten 5 entradas por transacción.
        // Esta restricción tiene prioridad sobre RN-05 (limitación por precio).
        if (hoursUntilStart < LastMinuteWindowHours && quantity > LastMinuteMaxQuantity)
        {
            throw new DomainException(
                $"A menos de {LastMinuteWindowHours} horas del inicio solo se permiten {LastMinuteMaxQuantity} entradas por transacción.");
        }

        // RN-05: eventos con precio > $100 limitan a 10 entradas por transacción.
        if (ticketPrice > HighPriceThreshold && quantity > HighPriceMaxQuantity)
        {
            throw new BusinessRuleException(RuleMessageKeys.Rn05PriceTicketLimit, HighPriceThreshold, HighPriceMaxQuantity);
        }

        var reservation = new Reservation(
            eventId, quantity, purchaserName.Trim(), email, city?.Trim(), reservationStatusId);
        reservation.AddDomainEvent(new ReservationCreatedEvent(reservation));
        return reservation;
    }

    /// <summary>
    /// Confirma el pago (RF-04): pasa a "confirmada" y asigna el código único. Los ids de
    /// los estados los resuelve el handler contra el catálogo.
    /// </summary>
    public void ConfirmPayment(ReservationCode code, Guid confirmedStatusId, Guid cancelledStatusId)
    {
        ArgumentNullException.ThrowIfNull(code);

        if (ReservationStatusId == confirmedStatusId)
        {
            throw new DomainException("La reserva ya está confirmada.");
        }
        {

        if (ReservationStatusId == cancelledStatusId)
            throw new DomainException("No se puede confirmar una reserva cancelada.");
        }

        ReservationStatusId = confirmedStatusId;
        Code = code;
        AddDomainEvent(new ReservationConfirmedEvent(this));
    }

    /// <summary>
    /// Cancela la reserva (RF-05). Solo es posible desde "confirmada". Los ids de los estados
    /// los resuelve el handler contra el catálogo.
    /// </summary>
    /// <param name="eventStartDate">
    /// Inicio del evento. RN-07: si faltan menos de 48 horas, la cancelación penaliza y las
    /// entradas se marcan como perdidas (no se liberan para la venta).
    /// </param>
    public void Cancel(
        DateTimeOffset utcNow,
        Guid cancelledStatusId,
        Guid pendingStatusId,
        DateTimeOffset eventStartDate)
    {
        if (ReservationStatusId == cancelledStatusId)
        {
            throw new DomainException("La reserva ya está cancelada.");
        }

        if (ReservationStatusId == pendingStatusId)
        {
            throw new DomainException("No se puede cancelar una reserva pendiente de pago.");
        }

        // RN-07: penalización si faltan menos de 48 horas para el inicio del evento.
        var penalized = (eventStartDate - utcNow).TotalHours < PenaltyWindowHours;

        ReservationStatusId = cancelledStatusId;
        CancelledAt = utcNow;
        IsForfeited = penalized;
        AddDomainEvent(new ReservationCancelledEvent(this, penalized));
    }
}
