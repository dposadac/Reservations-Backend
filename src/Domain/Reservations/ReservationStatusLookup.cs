namespace Ceiba.LiveEvent.Reservations.Domain.Reservations;

/// <summary>
/// Proyección de solo lectura de la tabla maestra <c>reservation_status</c>. Permite
/// listar/consultar los estados de reserva sin acoplar el dominio a sus UUID.
/// </summary>
public sealed class ReservationStatusLookup
{
    // EF Core necesita un constructor sin parámetros.
    private ReservationStatusLookup()
    {
    }

    public Guid Id { get; private set; }

    /// <summary>Nombre del estado tal como está en el catálogo (p. ej. "pendiente_pago").</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Crea una instancia (uso en pruebas o siembra controlada).</summary>
    public static ReservationStatusLookup Create(string name, Guid? id = null)
        => new() { Id = id ?? Guid.NewGuid(), Name = name };
}
