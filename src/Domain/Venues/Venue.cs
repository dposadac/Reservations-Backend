using Ceiba.LiveEvent.Reservations.Domain.Common;
using Ceiba.LiveEvent.Reservations.Domain.Exceptions;

namespace Ceiba.LiveEvent.Reservations.Domain.Venues;

/// <summary>
/// Lugar (sede) donde se celebran los eventos. Tabla maestra <c>venue</c>.
/// </summary>
public class Venue : BaseEntity, IAggregateRoot
{
    // EF Core / serializadores necesitan un constructor sin parámetros.
    private Venue()
    {
    }

    private Venue(string venueName, int quantity, string? city)
    {
        VenueName = venueName;
        Quantity = quantity;
        City = city;
    }

    /// <summary>Nombre del lugar.</summary>
    public string VenueName { get; private set; } = string.Empty;

    /// <summary>Capacidad (aforo) del lugar.</summary>
    public int Quantity { get; private set; }

    /// <summary>Ciudad donde se ubica el lugar.</summary>
    public string? City { get; private set; }

    public static Venue Create(string venueName, int quantity, string? city = null)
    {
        if (string.IsNullOrWhiteSpace(venueName))
        {
            throw new DomainException("El nombre del lugar es obligatorio.");
        }

        if (quantity < 0)
        {
            throw new DomainException("La capacidad del lugar no puede ser negativa.");
        }

        return new Venue(venueName.Trim(), quantity, city?.Trim());
    }
}
