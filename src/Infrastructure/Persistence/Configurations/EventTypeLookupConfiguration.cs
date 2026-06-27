using Ceiba.LiveEvent.Reservations.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ceiba.LiveEvent.Reservations.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeo de <see cref="EventTypeLookup"/> a la tabla maestra <c>event_type</c>, para
/// poder consultarla por nombre. Alineado con <c>db/scripts/03_create_table_event_type.sql</c>.
/// </summary>
public sealed class EventTypeLookupConfiguration : IEntityTypeConfiguration<EventTypeLookup>
{
    public void Configure(EntityTypeBuilder<EventTypeLookup> builder)
    {
        builder.ToTable("event_type");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasColumnName("event_type_name")
            .HasMaxLength(50)
            .IsRequired();
    }
}
