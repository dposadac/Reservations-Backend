using Ceiba.LiveEvent.Reservations.Api.Common;
using Ceiba.LiveEvent.Reservations.Api.Messages;
using Ceiba.LiveEvent.Reservations.Application.Reservations.Commands.CancelReservation;
using Ceiba.LiveEvent.Reservations.Application.Reservations.Commands.ConfirmReservationPayment;
using Ceiba.LiveEvent.Reservations.Application.Reservations.Commands.CreateReservation;
using Ceiba.LiveEvent.Reservations.Application.Reservations.Dtos;
using Ceiba.LiveEvent.Reservations.Application.Reservations.Queries.GetReservations;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ceiba.LiveEvent.Reservations.Api.Controllers;

/// <summary>
/// Endpoints de gestión de reservas. El controlador solo orquesta: delega toda la
/// lógica a la capa de aplicación a través de MediatR.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class ReservationController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMessageService _messages;
    private readonly IValidator<CreateReservationCommand> _createReservationValidator;

    public ReservationController(
        ISender sender,
        IMessageService messages,
        IValidator<CreateReservationCommand> createReservationValidator)
    {
        _sender = sender;
        _messages = messages;
        _createReservationValidator = createReservationValidator;
    }

    /// <summary>Lista reservas con filtros opcionales por evento y estado.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ReservationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReservationDto>>> GetAll(
        [FromQuery] GetReservationsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Reserva entradas de un evento (RF-03).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Guid>>> Create(
        CreateReservationCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createReservationValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(string.Join(" | ", errorMessages));
        }

        var id = await _sender.Send(command, cancellationToken);
        var body = ApiResponse<Guid>.Of(_messages.Get("reservation.created"), id);
        return CreatedAtAction(nameof(GetAll), null, body);
    }

    /// <summary>Confirma el pago de una reserva y devuelve el código generado (RF-04).</summary>
    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<string>>> Confirm(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var code = await _sender.Send(new ConfirmReservationPaymentCommand(id), cancellationToken);
            return Ok(ApiResponse<string>.Of(_messages.Get("reservation.confirmed", code), code));
        }
        catch (Exception ex)
        {

            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>Cancela una reserva (RF-05). La penalización (RN-07) la decide el dominio.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var penalized = await _sender.Send(new CancelReservationCommand(id), cancellationToken);
        var messageKey = penalized ? "reservation.cancelledWithPenalty" : "reservation.cancelled";
        return Ok(ApiResponse<object>.Of(_messages.Get(messageKey)));
    }
}
