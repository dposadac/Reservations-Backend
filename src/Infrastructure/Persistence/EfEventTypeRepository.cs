using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Ceiba.LiveEvent.Reservations.Infrastructure.Persistence;

/// <summary>Implementación de <see cref="IEventTypeRepository"/> con EF Core / PostgreSQL.</summary>
public sealed class EfEventTypeRepository : IEventTypeRepository
{
    private readonly ApplicationDbContext _context;

    public EfEventTypeRepository(ApplicationDbContext context) => _context = context;

    public async Task<EventTypeLookup?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalized = (name ?? string.Empty).Trim().ToLowerInvariant();

        return await _context.EventTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name.ToLower() == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<EventTypeLookup>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.EventTypes.AsNoTracking().ToListAsync(cancellationToken);
}
