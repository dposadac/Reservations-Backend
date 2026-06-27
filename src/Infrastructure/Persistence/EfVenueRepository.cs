using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Domain.Venues;
using Microsoft.EntityFrameworkCore;

namespace Ceiba.LiveEvent.Reservations.Infrastructure.Persistence;

/// <summary>Implementación de <see cref="IVenueRepository"/> con EF Core / PostgreSQL.</summary>
public sealed class EfVenueRepository : IVenueRepository
{
    private readonly ApplicationDbContext _context;

    public EfVenueRepository(ApplicationDbContext context) => _context = context;

    public async Task<Venue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Venues.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Venue>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Venues.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(Venue item, CancellationToken cancellationToken = default)
        => await _context.Venues.AddAsync(item, cancellationToken);

    public void Update(Venue item) => _context.Venues.Update(item);

    public void Remove(Venue item) => _context.Venues.Remove(item);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
