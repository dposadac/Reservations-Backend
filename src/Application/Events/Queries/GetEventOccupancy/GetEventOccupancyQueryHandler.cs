using Ceiba.LiveEvent.Reservations.Application.Common.Exceptions;
using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Application.Events.Dtos;
using Ceiba.LiveEvent.Reservations.Domain.Events;
using Ceiba.LiveEvent.Reservations.Domain.Reservations;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Events.Queries.GetEventOccupancy;

public sealed class GetEventOccupancyQueryHandler
    : IRequestHandler<GetEventOccupancyQuery, OccupancyReportDto>
{
    private readonly IEventRepository _events;
    private readonly IReservationRepository _reservations;
    private readonly IEventStatusRepository _eventStatuses;
    private readonly IReservationStatusRepository _reservationStatuses;
    private readonly IDateTimeProvider _clock;

    public GetEventOccupancyQueryHandler(
        IEventRepository events,
        IReservationRepository reservations,
        IEventStatusRepository eventStatuses,
        IReservationStatusRepository reservationStatuses,
        IDateTimeProvider clock)
    {
        _events = events;
        _reservations = reservations;
        _eventStatuses = eventStatuses;
        _reservationStatuses = reservationStatuses;
        _clock = clock;
    }

    public async Task<OccupancyReportDto> Handle(
        GetEventOccupancyQuery request,
        CancellationToken cancellationToken)
    {
        var @event = await _events.GetByIdAsync(request.EventId, cancellationToken)
            ?? throw new NotFoundException(nameof(Event), request.EventId);

        // Refresca el estado en memoria (RN-06) para que el reporte refleje "completado"
        // si el evento ya finalizó. No se persiste: es una consulta de solo lectura. Los
        // ids de "activo" y "completado" se resuelven contra el catálogo.
        var eventStatuses = await _eventStatuses.GetAllAsync(cancellationToken);
        var statusNames = eventStatuses.ToDictionary(x => x.Id, x => x.Name);

        var activeName = EventStatus.Activo.ToString();
        var completedName = EventStatus.Completado.ToString();
        var active = eventStatuses.FirstOrDefault(
            x => string.Equals(x.Name, activeName, StringComparison.OrdinalIgnoreCase))
            ?? throw new NotFoundException(nameof(EventStatus), activeName);
        var completed = eventStatuses.FirstOrDefault(
            x => string.Equals(x.Name, completedName, StringComparison.OrdinalIgnoreCase))
            ?? throw new NotFoundException(nameof(EventStatus), completedName);

        @event.RefreshStatus(active.Id, completed.Id, _clock.UtcNow);

        // El conteo de vendidas usa el id de "confirmada" del catálogo.
        var confirmedStatus = await _reservationStatuses.GetByNameAsync(
            ReservationStatusNames.Confirmed, cancellationToken)
            ?? throw new NotFoundException(nameof(ReservationStatus), ReservationStatusNames.Confirmed);

        var reservations = await _reservations.GetByEventAsync(@event.Id, cancellationToken);

        var report = OccupancyReport.Create(@event, reservations, confirmedStatus.Id);
        return OccupancyReportDto.FromReport(
            report,
            statusNames.GetValueOrDefault(report.EventStatusId, string.Empty));
    }
}
