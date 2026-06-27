using System.Reflection;
using Ceiba.LiveEvent.Reservations.Domain.Common;
using Ceiba.LiveEvent.Reservations.Domain.Events;
using Ceiba.LiveEvent.Reservations.Domain.Reservations;
using Ceiba.LiveEvent.Reservations.Domain.Todos;
using Ceiba.LiveEvent.Reservations.Domain.Venues;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ceiba.LiveEvent.Reservations.Infrastructure.Persistence;

/// <summary>
/// Contexto de EF Core (PostgreSQL) en modo Database First: el esquema lo gestionan los
/// scripts SQL de <c>db/scripts</c>, no las migraciones. El contexto solo mapea las
/// entidades a las tablas existentes y se encarga de la auditoría y del despacho de
/// eventos de dominio al guardar.
/// </summary>
public sealed class ApplicationDbContext : DbContext
{
    private readonly IPublisher _publisher;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IPublisher publisher)
        : base(options)
        => _publisher = publisher;

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public DbSet<Venue> Venues => Set<Venue>();

    public DbSet<EventTypeLookup> EventTypes => Set<EventTypeLookup>();

    public DbSet<EventStatusLookup> EventStatuses => Set<EventStatusLookup>();

    public DbSet<ReservationStatusLookup> ReservationStatuses => Set<ReservationStatusLookup>();

    public DbSet<Event> Events => Set<Event>();

    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();

        // Captura los eventos antes de guardar; se publican una vez persistido el cambio.
        var domainEntities = ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }

        domainEntities.ForEach(e => e.ClearDomainEvents());

        return result;
    }

    private void ApplyAuditInformation()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedAt = now;
                    break;
            }
        }
    }
}
