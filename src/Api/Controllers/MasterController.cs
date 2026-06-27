using Ceiba.LiveEvent.Reservations.Application.Masters.Dtos;
using Ceiba.LiveEvent.Reservations.Application.Masters.Queries.GetMasters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ceiba.LiveEvent.Reservations.Api.Controllers;

/// <summary>
/// Endpoint único de tablas maestras. Devuelve, en una sola respuesta, los tipos de
/// evento, estados de evento, estados de reserva y lugares.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class MasterController : ControllerBase
{
    private readonly ISender _sender;

    public MasterController(ISender sender) => _sender = sender;

    /// <summary>Devuelve todas las tablas maestras (sin filtros).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(MastersDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MastersDto>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMastersQuery(), cancellationToken);
        return Ok(result);
    }
}
