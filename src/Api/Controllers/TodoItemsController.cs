using Ceiba.LiveEvent.Reservations.Application.Todos.Commands.CompleteTodoItem;
using Ceiba.LiveEvent.Reservations.Application.Todos.Commands.CreateTodoItem;
using Ceiba.LiveEvent.Reservations.Application.Todos.Commands.DeleteTodoItem;
using Ceiba.LiveEvent.Reservations.Application.Todos.Commands.UpdateTodoItem;
using Ceiba.LiveEvent.Reservations.Application.Todos.Dtos;
using Ceiba.LiveEvent.Reservations.Application.Todos.Queries.GetTodoItemById;
using Ceiba.LiveEvent.Reservations.Application.Todos.Queries.GetTodoItems;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ceiba.LiveEvent.Reservations.Api.Controllers;

/// <summary>
/// Endpoints de gestión de tareas (TODO). El controlador solo orquesta: delega toda
/// la lógica a la capa de aplicación a través de MediatR.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class TodoItemsController : ControllerBase
{
    private readonly ISender _sender;

    public TodoItemsController(ISender sender) => _sender = sender;

    /// <summary>Obtiene el listado de tareas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TodoItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TodoItemDto>>> GetAll(
        [FromQuery] bool? onlyPending,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTodoItemsQuery(onlyPending), cancellationToken);
        return Ok(result);
    }

    /// <summary>Obtiene una tarea por su identificador.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TodoItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodoItemDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTodoItemByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Crea una nueva tarea.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create(
        CreateTodoItemCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>Actualiza los detalles de una tarea.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateTodoItemCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest("El identificador de la ruta no coincide con el del cuerpo.");
        }

        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>Marca una tarea como completada.</summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new CompleteTodoItemCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>Elimina una tarea.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteTodoItemCommand(id), cancellationToken);
        return NoContent();
    }
}
