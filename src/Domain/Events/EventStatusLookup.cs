namespace Ceiba.LiveEvent.Reservations.Domain.Events;

/// <summary>
/// Proyección de solo lectura de la tabla maestra <c>event_status</c>. Permite resolver
/// un estado de evento por su nombre (texto) sin acoplar el dominio a los UUID del catálogo.
/// </summary>
public sealed class EventStatusLookup
{
    // EF Core necesita un constructor sin parámetros.
    private EventStatusLookup()
    {
    }

    public Guid Id { get; private set; }

    /// <summary>Nombre del estado tal como está en el catálogo (p. ej. "activo").</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Crea una instancia (uso en pruebas o siembra controlada).</summary>
    public static EventStatusLookup Create(string name, Guid? id = null)
        => new() { Id = id ?? Guid.NewGuid(), Name = name };
}
