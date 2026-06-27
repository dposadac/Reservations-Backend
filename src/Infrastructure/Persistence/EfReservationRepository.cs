using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Domain.Reservations;
using Microsoft.EntityFrameworkCore;

namespace Ceiba.LiveEvent.Reservations.Infrastructure.Persistence;

/// <summary>Implementación de <see cref="IReservationRepository"/> con EF Core / PostgreSQL.</summary>
public sealed class EfReservationRepository : IReservationRepository
{
    private readonly ApplicationDbContext _context;

    public EfReservationRepository(ApplicationDbContext context) => _context = context;

    public async Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Reservations.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Reservation>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Reservations.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Reservation>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default)
        => await _context.Reservations.AsNoTracking()
            .Where(x => x.EventId == eventId)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var probe = ReservationCode.FromTrusted(code);
        return await _context.Reservations.AnyAsync(x => x.Code == probe, cancellationToken);
    }

    public async Task AddAsync(Reservation item, CancellationToken cancellationToken = default)
        => await _context.Reservations.AddAsync(item, cancellationToken);

    public void Update(Reservation item) => _context.Reservations.Update(item);

    public void Remove(Reservation item) => _context.Reservations.Remove(item);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
