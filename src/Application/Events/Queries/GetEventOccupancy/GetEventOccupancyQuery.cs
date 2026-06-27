using Ceiba.LiveEvent.Reservations.Application.Events.Dtos;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Events.Queries.GetEventOccupancy;

/// <summary>Consulta del reporte de ocupación de un evento (RF-06).</summary>
public sealed record GetEventOccupancyQuery(Guid EventId) : IRequest<OccupancyReportDto>;
