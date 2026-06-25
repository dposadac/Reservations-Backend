namespace Ceiba.LiveEvent.Reservations.Domain.Common;

/// <summary>
/// Entidad base auditable. Añade información de creación y última modificación.
/// </summary>
public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? LastModifiedAt { get; set; }
}
