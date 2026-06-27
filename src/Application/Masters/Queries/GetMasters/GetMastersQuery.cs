using Ceiba.LiveEvent.Reservations.Application.Masters.Dtos;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Application.Masters.Queries.GetMasters;

/// <summary>Consulta única que devuelve todas las tablas maestras (sin filtros).</summary>
public sealed record GetMastersQuery : IRequest<MastersDto>;
