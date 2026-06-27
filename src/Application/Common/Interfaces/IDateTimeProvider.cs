namespace Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;

/// <summary>
/// Abstracción del reloj del sistema. Permite que la lógica que depende de la hora
/// actual (fechas futuras, ventana de última hora, etc.) sea determinista y testeable.
/// </summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
