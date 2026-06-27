using System.Text.RegularExpressions;
using Ceiba.LiveEvent.Reservations.Domain.Exceptions;

namespace Ceiba.LiveEvent.Reservations.Domain.Common;

/// <summary>
/// Value object que representa un correo electrónico con formato válido.
/// </summary>
public sealed partial class Email : IEquatable<Email>
{
    private Email(string value) => Value = value;

    public string Value { get; }

    /// <summary>Crea un <see cref="Email"/> validando su formato.</summary>
    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("El correo electrónico es obligatorio.");
        }

        var normalized = value.Trim();

        if (normalized.Length > 150 || !EmailRegex().IsMatch(normalized))
        {
            throw new DomainException($"El correo electrónico '{value}' no tiene un formato válido.");
        }

        return new Email(normalized);
    }

    /// <summary>Rehidrata un valor ya validado desde almacenamiento (sin re-validar).</summary>
    public static Email FromTrusted(string value) => new(value);

    public bool Equals(Email? other)
        => other is not null && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => Equals(obj as Email);

    public override int GetHashCode() => Value.ToLowerInvariant().GetHashCode();

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();
}
