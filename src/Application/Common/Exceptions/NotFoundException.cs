namespace Ceiba.LiveEvent.Reservations.Application.Common.Exceptions;

/// <summary>Se lanza cuando no se encuentra una entidad solicitada.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string name, object key)
        : base($"La entidad \"{name}\" ({key}) no fue encontrada.")
    {
    }
}
