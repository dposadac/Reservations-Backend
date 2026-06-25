using Ceiba.LiveEvent.Reservations.Application.Todos.Commands.CreateTodoItem;
using Ceiba.LiveEvent.Reservations.Domain.Todos.Enums;
using FluentAssertions;

namespace Ceiba.LiveEvent.Reservations.Application.Tests.Todos.Commands;

public class CreateTodoItemCommandValidatorTests
{
    private readonly CreateTodoItemCommandValidator _validator = new();

    [Fact]
    public void Validate_ConComandoValido_NoDevuelveErrores()
    {
        var command = new CreateTodoItemCommand { Title = "Tarea válida", Priority = PriorityLevel.Low };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ConTituloVacio_DevuelveError()
    {
        var command = new CreateTodoItemCommand { Title = string.Empty };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateTodoItemCommand.Title));
    }

    [Fact]
    public void Validate_ConTituloDemasiadoLargo_DevuelveError()
    {
        var command = new CreateTodoItemCommand { Title = new string('a', 201) };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
