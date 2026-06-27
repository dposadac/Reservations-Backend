namespace Ceiba.LiveEvent.Reservations.Domain.Events;

/// <summary>
/// Proyección de solo lectura de la tabla maestra <c>event_type</c>. Permite resolver
/// un tipo de evento por su nombre (texto) sin acoplar el dominio a los UUID del catálogo.
/// El dominio sigue usando el enum <see cref="EventType"/>; esta entidad es para consultas.
/// </summary>
public sealed class EventTypeLookup
{
    // EF Core necesita un constructor sin parámetros.
    private EventTypeLookup()
    {
    }

    public Guid Id { get; private set; }

    /// <summary>Nombre del tipo de evento tal como está en el catálogo (p. ej. "conferencia").</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Crea una instancia (uso en pruebas o siembra controlada).</summary>
    public static EventTypeLookup Create(string name, Guid? id = null)
        => new() { Id = id ?? Guid.NewGuid(), Name = name };
}
