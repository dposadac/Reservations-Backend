using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Ceiba.LiveEvent.Reservations.Infrastructure.Persistence;

/// <summary>Implementación de <see cref="IEventStatusRepository"/> con EF Core / PostgreSQL.</summary>
public sealed class EfEventStatusRepository : IEventStatusRepository
{
    private readonly ApplicationDbContext _context;

    public EfEventStatusRepository(ApplicationDbContext context) => _context = context;

    public async Task<EventStatusLookup?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalized = (name ?? string.Empty).Trim().ToLowerInvariant();

        return await _context.EventStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name.ToLower() == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<EventStatusLookup>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.EventStatuses.AsNoTracking().ToListAsync(cancellationToken);
}
