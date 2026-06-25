using Ceiba.LiveEvent.Reservations.Domain.Todos;

namespace Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;

/// <summary>
/// Abstracción de persistencia para la raíz de agregado <see cref="TodoItem"/>.
/// Definida en la capa de aplicación e implementada en infraestructura (DIP).
/// </summary>
public interface ITodoRepository
{
    Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(TodoItem item, CancellationToken cancellationToken = default);

    void Update(TodoItem item);

    void Remove(TodoItem item);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
