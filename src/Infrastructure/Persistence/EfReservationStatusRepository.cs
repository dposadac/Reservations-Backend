using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Domain.Reservations;
using Microsoft.EntityFrameworkCore;

namespace Ceiba.LiveEvent.Reservations.Infrastructure.Persistence;

/// <summary>Implementación de <see cref="IReservationStatusRepository"/> con EF Core / PostgreSQL.</summary>
public sealed class EfReservationStatusRepository : IReservationStatusRepository
{
    private readonly ApplicationDbContext _context;

    public EfReservationStatusRepository(ApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<ReservationStatusLookup>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.ReservationStatuses.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<ReservationStatusLookup?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalized = (name ?? string.Empty).Trim().ToLowerInvariant();

        return await _context.ReservationStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name.ToLower() == normalized, cancellationToken);
    }
}
