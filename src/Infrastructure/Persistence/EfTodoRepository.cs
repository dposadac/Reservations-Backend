using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Domain.Todos;
using Microsoft.EntityFrameworkCore;

namespace Ceiba.LiveEvent.Reservations.Infrastructure.Persistence;

/// <summary>Implementación de <see cref="ITodoRepository"/> con EF Core / PostgreSQL.</summary>
public sealed class EfTodoRepository : ITodoRepository
{
    private readonly ApplicationDbContext _context;

    public EfTodoRepository(ApplicationDbContext context) => _context = context;

    public async Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.TodoItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.TodoItems.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(TodoItem item, CancellationToken cancellationToken = default)
        => await _context.TodoItems.AddAsync(item, cancellationToken);

    public void Update(TodoItem item) => _context.TodoItems.Update(item);

    public void Remove(TodoItem item) => _context.TodoItems.Remove(item);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
