using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;

namespace Ceiba.LiveEvent.Reservations.Infrastructure.Common;

/// <summary>Implementación de <see cref="IDateTimeProvider"/> basada en el reloj del sistema.</summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
