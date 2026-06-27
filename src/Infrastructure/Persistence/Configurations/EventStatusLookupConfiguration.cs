using Ceiba.LiveEvent.Reservations.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ceiba.LiveEvent.Reservations.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeo de <see cref="EventStatusLookup"/> a la tabla maestra <c>event_status</c>, para
/// poder consultarla por nombre. Alineado con <c>db/scripts/01_create_table_event_status.sql</c>.
/// </summary>
public sealed class EventStatusLookupConfiguration : IEntityTypeConfiguration<EventStatusLookup>
{
    public void Configure(EntityTypeBuilder<EventStatusLookup> builder)
    {
        builder.ToTable("event_status");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasColumnName("status_name")
            .HasMaxLength(100)
            .IsRequired();

        // La columna description existe en la tabla pero no se mapea (no se necesita aquí).
    }
}
