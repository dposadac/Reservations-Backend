using System.ComponentModel.DataAnnotations.Schema;

namespace Ceiba.LiveEvent.Reservations.Domain.Common;

/// <summary>
/// Entidad base. Aporta identidad y la capacidad de registrar eventos de dominio.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    private readonly List<BaseEvent> _domainEvents = new();

    /// <summary>Eventos de dominio pendientes de despachar.</summary>
    [NotMapped]
    public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(BaseEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void RemoveDomainEvent(BaseEvent domainEvent) => _domainEvents.Remove(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
