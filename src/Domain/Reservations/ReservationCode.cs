using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Ceiba.LiveEvent.Reservations.Domain.Exceptions;

namespace Ceiba.LiveEvent.Reservations.Domain.Reservations;

/// <summary>
/// Value object para el código único de reserva. Formato <c>EV-{6 dígitos}</c> (RF-04).
/// </summary>
public sealed partial class ReservationCode : IEquatable<ReservationCode>
{
    private ReservationCode(string value) => Value = value;

    public string Value { get; }

    /// <summary>Genera un nuevo código aleatorio con el formato <c>EV-######</c>.</summary>
    /// <remarks>
    /// La unicidad debe garantizarla la capa de aplicación (reintentando si colisiona),
    /// ya que el dominio no tiene acceso al almacenamiento.
    /// </remarks>
    public static ReservationCode New()
    {
        var number = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return new ReservationCode($"EV-{number:D6}");
    }

    /// <summary>Crea un código validando su formato.</summary>
    public static ReservationCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !CodeRegex().IsMatch(value.Trim()))
        {
            throw new DomainException($"El código de reserva '{value}' no tiene el formato 'EV-######'.");
        }

        return new ReservationCode(value.Trim());
    }

    /// <summary>Rehidrata un valor ya validado desde almacenamiento (sin re-validar).</summary>
    public static ReservationCode FromTrusted(string value) => new(value);

    public bool Equals(ReservationCode? other)
        => other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => Equals(obj as ReservationCode);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;

    [GeneratedRegex(@"^EV-\d{6}$", RegexOptions.CultureInvariant)]
    private static partial Regex CodeRegex();
}
