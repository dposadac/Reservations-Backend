using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;

namespace Ceiba.LiveEvent.Reservations.Application.Tests.Common;

/// <summary>Reloj determinista para las pruebas de la capa de aplicación.</summary>
public sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public FakeDateTimeProvider(DateTimeOffset utcNow) => UtcNow = utcNow;

    public DateTimeOffset UtcNow { get; set; }
}
