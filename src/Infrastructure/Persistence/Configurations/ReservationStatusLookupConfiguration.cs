using Ceiba.LiveEvent.Reservations.Domain.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ceiba.LiveEvent.Reservations.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeo de <see cref="ReservationStatusLookup"/> a la tabla maestra <c>reservation_status</c>.
/// Alineado con <c>db/scripts/04_create_table_reservation_status.sql</c>.
/// </summary>
public sealed class ReservationStatusLookupConfiguration : IEntityTypeConfiguration<ReservationStatusLookup>
{
    public void Configure(EntityTypeBuilder<ReservationStatusLookup> builder)
    {
        builder.ToTable("reservation_status");

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
