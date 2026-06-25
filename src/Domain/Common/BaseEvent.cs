using MediatR;

namespace Ceiba.LiveEvent.Reservations.Domain.Common;

/// <summary>
/// Clase base para los eventos de dominio. Implementa <see cref="INotification"/>
/// para poder ser publicados a través de MediatR.
/// </summary>
public abstract class BaseEvent : INotification
{
}
