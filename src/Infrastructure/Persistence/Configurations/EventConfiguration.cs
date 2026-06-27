using Ceiba.LiveEvent.Reservations.Domain.Events;
using Ceiba.LiveEvent.Reservations.Domain.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ceiba.LiveEvent.Reservations.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeo de <see cref="Event"/> a la tabla <c>event</c>.
/// Debe mantenerse alineado con <c>db/scripts/05_create_table_event.sql</c>.
/// </summary>
public sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("event");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("event_id")
            .ValueGeneratedNever();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(Event.TitleMaxLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(Event.DescriptionMaxLength)
            .IsRequired();

        builder.Property(x => x.VenueId)
            .HasColumnName("venue_id")
            .IsRequired();

        builder.Property(x => x.MaximumCapacity)
            .HasColumnName("maximum_capacity")
            .IsRequired();

        builder.Property(x => x.StartDate)
            .HasColumnName("start_date")
            .IsRequired();

        builder.Property(x => x.EndDate)
            .HasColumnName("end_date")
            .IsRequired();

        builder.Property(x => x.TicketPrice)
            .HasColumnName("ticket_price")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.EventTypeId)
            .HasColumnName("event_type_id")
            .IsRequired();

        builder.Property(x => x.EventStatusId)
            .HasColumnName("status_event_id")
            .IsRequired();

        // FK al lugar (Venue es la única tabla maestra con entidad de dominio).
        builder.HasOne<Venue>()
            .WithMany()
            .HasForeignKey(x => x.VenueId)
            .HasConstraintName("fk_event_venue")
            .OnDelete(DeleteBehavior.Restrict);

        // Tipo y estado se guardan como FK uuid; la integridad la garantiza la BD
        // (fk_event_event_type / fk_event_event_status). Solo se indexan las columnas.
        builder.HasIndex(x => x.VenueId, "ix_event_venue_id");
        builder.HasIndex(x => x.EventTypeId, "ix_event_event_type_id");
        builder.HasIndex(x => x.EventStatusId, "ix_event_status_event_id");

        // Los eventos de dominio no se persisten.
        builder.Ignore(x => x.DomainEvents);
    }
}
