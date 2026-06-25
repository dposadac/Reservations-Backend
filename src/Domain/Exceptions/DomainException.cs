namespace Ceiba.LiveEvent.Reservations.Domain.Exceptions;

/// <summary>
/// Excepción que representa la violación de una invariante o regla de negocio del dominio.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
