using Ceiba.LiveEvent.Reservations.Application.Common.Behaviours;
using Ceiba.LiveEvent.Reservations.Application.Todos.Commands.CreateTodoItem;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using ValidationException = Ceiba.LiveEvent.Reservations.Application.Common.Exceptions.ValidationException;

namespace Ceiba.LiveEvent.Reservations.Application.Tests.Common.Behaviours;

public class ValidationBehaviourTests
{
    private readonly Mock<RequestHandlerDelegate<Guid>> _next = new();

    [Fact]
    public async Task Handle_CuandoEsValido_InvocaElSiguienteHandler()
    {
        var expected = Guid.NewGuid();
        _next.Setup(n => n()).ReturnsAsync(expected);

        var validators = new IValidator<CreateTodoItemCommand>[] { new CreateTodoItemCommandValidator() };
        var behaviour = new ValidationBehaviour<CreateTodoItemCommand, Guid>(validators);
        var command = new CreateTodoItemCommand { Title = "Tarea válida" };

        var result = await behaviour.Handle(command, _next.Object, CancellationToken.None);

        result.Should().Be(expected);
        _next.Verify(n => n(), Times.Once);
    }

    [Fact]
    public async Task Handle_CuandoEsInvalido_LanzaValidationExceptionYNoInvocaElHandler()
    {
        var validators = new IValidator<CreateTodoItemCommand>[] { new CreateTodoItemCommandValidator() };
        var behaviour = new ValidationBehaviour<CreateTodoItemCommand, Guid>(validators);
        var command = new CreateTodoItemCommand { Title = string.Empty };

        var act = () => behaviour.Handle(command, _next.Object, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        _next.Verify(n => n(), Times.Never);
    }
}
