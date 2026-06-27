using Ceiba.LiveEvent.Reservations.Api.Common;
using Ceiba.LiveEvent.Reservations.Api.Messages;
using Ceiba.LiveEvent.Reservations.Application.Events.Commands.CancelEvent;
using Ceiba.LiveEvent.Reservations.Application.Events.Commands.CompleteFinishedEvents;
using Ceiba.LiveEvent.Reservations.Application.Events.Commands.CreateEvent;
using Ceiba.LiveEvent.Reservations.Application.Events.Dtos;
using Ceiba.LiveEvent.Reservations.Application.Events.Queries.GetEventById;
using Ceiba.LiveEvent.Reservations.Application.Events.Queries.GetEventOccupancy;
using Ceiba.LiveEvent.Reservations.Application.Events.Queries.GetEvents;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ceiba.LiveEvent.Reservations.Api.Controllers;

/// <summary>
/// Endpoints de gestión de eventos. El controlador solo orquesta: delega toda la
/// lógica a la capa de aplicación a través de MediatR.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class EventController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMessageService _messages;
    private readonly IValidator<CreateEventCommand> _createEventValidator;

    public EventController(
        ISender sender,
        IMessageService messages,
        IValidator<CreateEventCommand> createEventValidator)
    {
        _sender = sender;
        _messages = messages;
        _createEventValidator = createEventValidator;
    }

    /// <summary>Lista eventos con filtros opcionales (RF-02).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EventDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> GetAll(
        [FromQuery] GetEventsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Obtiene un evento por su identificador.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetEventByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Reporte de ocupación de un evento (RF-06). Se invoca por POST enviando el id en la ruta.</summary>
    [HttpGet("{id:guid}/occupancy")]
    [ProducesResponseType(typeof(OccupancyReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OccupancyReportDto>> GetOccupancy(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetEventOccupancyQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Crea un evento (RF-01).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Guid>>> Create(
        CreateEventCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createEventValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(string.Join(" | ", errorMessages));
        }

        try
        {
            var id = await _sender.Send(command, cancellationToken);
            var body = ApiResponse<Guid>.Of(_messages.Get("event.created"), id);
            return CreatedAtAction(nameof(GetById), new { id }, body);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Of(ex.Message));
        }
    }

    /// <summary>Cancela un evento (RN-06).</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new CancelEventCommand(id), cancellationToken);
        return Ok(ApiResponse<object>.Of(_messages.Get("event.cancelled")));
    }

    /// <summary>
    /// Marca como "completado" los eventos cuya fecha de fin ya pasó (RN-06). Pensado para
    /// ejecutarse a diario: lo invoca el job en segundo plano y también puede dispararse aquí
    /// (p. ej. desde un scheduler externo como Cloud Scheduler).
    /// </summary>
    [HttpPost("complete-finished")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<int>>> CompleteFinished(CancellationToken cancellationToken)
    {
        var count = await _sender.Send(new CompleteFinishedEventsCommand(), cancellationToken);
        return Ok(ApiResponse<int>.Of(_messages.Get("event.completedFinished", count), count));
    }
}
