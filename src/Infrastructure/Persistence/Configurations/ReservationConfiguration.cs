using Ceiba.LiveEvent.Reservations.Domain.Events;
using Ceiba.LiveEvent.Reservations.Domain.Reservations;
using Ceiba.LiveEvent.Reservations.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ceiba.LiveEvent.Reservations.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeo de <see cref="Reservation"/> a la tabla <c>reservation</c>.
/// Debe mantenerse alineado con <c>db/scripts/06_create_table_reservation.sql</c>.
/// </summary>
public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservation");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.EventId)
            .HasColumnName("event_id")
            .IsRequired();

        builder.Property(x => x.ReservationStatusId)
            .HasColumnName("reservation_status_id")
            .IsRequired();

        builder.Property(x => x.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(x => x.PurchaserName)
            .HasColumnName("purchaser_name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.PurchaserEmail)
            .HasColumnName("purchaser_email")
            .HasMaxLength(150)
            .HasConversion(new EmailConverter())
            .IsRequired();

        builder.Property(x => x.City)
            .HasColumnName("city")
            .HasMaxLength(50);

        // El código solo existe tras confirmar el pago; EF aplica el convertidor solo a valores no nulos.
        builder.Property(x => x.Code)
            .HasColumnName("reservation_code")
            .HasMaxLength(20)
            .HasConversion(
                code => code!.Value,
                value => ReservationCode.FromTrusted(value));

        builder.Property(x => x.CancelledAt)
            .HasColumnName("cancelled_at");

        builder.Property(x => x.IsForfeited)
            .HasColumnName("is_forfeited")
            .IsRequired();

        builder.HasOne<Event>()
            .WithMany()
            .HasForeignKey(x => x.EventId)
            .HasConstraintName("fk_reservation_event")
            .OnDelete(DeleteBehavior.Restrict);

        // El estado es un enum mapeado a FK uuid; la integridad la garantiza la BD
        // (fk_reservation_reservation_status). Solo se indexa la columna.
        builder.HasIndex(x => x.EventId, "ix_reservation_event_id");
        builder.HasIndex(x => x.ReservationStatusId, "ix_reservation_status_id");
        builder.HasIndex(x => x.Code, "ux_reservation_code")
            .IsUnique();

        // Los eventos de dominio no se persisten.
        builder.Ignore(x => x.DomainEvents);
    }
}
