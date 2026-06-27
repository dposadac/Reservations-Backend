using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Ceiba.LiveEvent.Reservations.Infrastructure.Persistence;

/// <summary>Implementación de <see cref="IEventRepository"/> con EF Core / PostgreSQL.</summary>
public sealed class EfEventRepository : IEventRepository
{
    private readonly ApplicationDbContext _context;

    public EfEventRepository(ApplicationDbContext context) => _context = context;

    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Events.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Events.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Event>> GetByVenueAsync(Guid venueId, CancellationToken cancellationToken = default)
        => await _context.Events.AsNoTracking()
            .Where(x => x.VenueId == venueId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Event item, CancellationToken cancellationToken = default)
        => await _context.Events.AddAsync(item, cancellationToken);

    public void Update(Event item) => _context.Events.Update(item);

    public void Remove(Event item) => _context.Events.Remove(item);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
