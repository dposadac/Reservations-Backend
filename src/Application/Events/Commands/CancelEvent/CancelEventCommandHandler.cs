using Ceiba.LiveEvent.Reservations.Application.Common.Exceptions;
using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Domain.Events;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Events.Commands.CancelEvent;

public sealed class CancelEventCommandHandler : IRequestHandler<CancelEventCommand>
{
    private readonly IEventRepository _events;
    private readonly IEventStatusRepository _eventStatuses;

    public CancelEventCommandHandler(IEventRepository events, IEventStatusRepository eventStatuses)
    {
        _events = events;
        _eventStatuses = eventStatuses;
    }

    public async Task Handle(CancelEventCommand request, CancellationToken cancellationToken)
    {
        var @event = await _events.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Event), request.Id);

        // Resuelve el id del estado "cancelado" contra el catálogo y lo aplica (RN-06).
        var cancelledName = EventStatus.Cancelado.ToString();
        var cancelledStatus = await _eventStatuses.GetByNameAsync(cancelledName, cancellationToken)
            ?? throw new NotFoundException(nameof(EventStatus), cancelledName);

        @event.Cancel(cancelledStatus.Id);

        _events.Update(@event);
        await _events.SaveChangesAsync(cancellationToken);
    }
}
