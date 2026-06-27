using Ceiba.LiveEvent.Reservations.Domain.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ceiba.LiveEvent.Reservations.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeo de <see cref="Venue"/> a la tabla <c>venue</c>.
/// Debe mantenerse alineado con <c>db/scripts/02_create_table_venue.sql</c>.
/// </summary>
public sealed class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("venue");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.VenueName)
            .HasColumnName("venue_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(x => x.City)
            .HasColumnName("city")
            .HasMaxLength(50);

        // Los eventos de dominio no se persisten.
        builder.Ignore(x => x.DomainEvents);
    }
}
