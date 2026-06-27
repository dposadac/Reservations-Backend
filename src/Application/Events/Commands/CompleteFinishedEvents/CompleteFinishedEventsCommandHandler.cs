using Ceiba.LiveEvent.Reservations.Application.Common.Exceptions;
using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Domain.Events;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Events.Commands.CompleteFinishedEvents;

public sealed class CompleteFinishedEventsCommandHandler
    : IRequestHandler<CompleteFinishedEventsCommand, int>
{
    private readonly IEventRepository _events;
    private readonly IEventStatusRepository _eventStatuses;
    private readonly IDateTimeProvider _clock;

    public CompleteFinishedEventsCommandHandler(
        IEventRepository events,
        IEventStatusRepository eventStatuses,
        IDateTimeProvider clock)
    {
        _events = events;
        _eventStatuses = eventStatuses;
        _clock = clock;
    }

    public async Task<int> Handle(CompleteFinishedEventsCommand request, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;

        var activeName = EventStatus.Activo.ToString();
        var completedName = EventStatus.Completado.ToString();
        var active = await _eventStatuses.GetByNameAsync(activeName, cancellationToken)
            ?? throw new NotFoundException(nameof(EventStatus), activeName);
        var completed = await _eventStatuses.GetByNameAsync(completedName, cancellationToken)
            ?? throw new NotFoundException(nameof(EventStatus), completedName);

        var events = await _events.GetAllAsync(cancellationToken);

        // RN-06: eventos activos cuya hora de fin ya pasó.
        var finished = events
            .Where(e => e.EventStatusId == active.Id && e.EndDate <= now)
            .ToList();

        foreach (var @event in finished)
        {
            @event.RefreshStatus(active.Id, completed.Id, now);
            _events.Update(@event);
        }

        if (finished.Count > 0)
        {
            await _events.SaveChangesAsync(cancellationToken);
        }

        return finished.Count;
    }
}
