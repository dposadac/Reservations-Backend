using FluentValidation.Results;

namespace Ceiba.LiveEvent.Reservations.Application.Common.Exceptions;

/// <summary>
/// Agrupa los errores de validación de FluentValidation en una única excepción
/// que la capa de presentación puede traducir a una respuesta HTTP 400.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException()
        : base("Se han producido uno o más errores de validación.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }

    public IDictionary<string, string[]> Errors { get; }
}
