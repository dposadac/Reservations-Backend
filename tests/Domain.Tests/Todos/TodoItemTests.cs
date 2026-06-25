using Ceiba.LiveEvent.Reservations.Domain.Exceptions;
using Ceiba.LiveEvent.Reservations.Domain.Todos;
using Ceiba.LiveEvent.Reservations.Domain.Todos.Enums;
using Ceiba.LiveEvent.Reservations.Domain.Todos.Events;
using FluentAssertions;

namespace Ceiba.LiveEvent.Reservations.Domain.Tests.Todos;

public class TodoItemTests
{
    [Fact]
    public void Create_ConDatosValidos_EstableceLasPropiedadesYRegistraEvento()
    {
        var item = TodoItem.Create("  Comprar entradas  ", "  Para el evento  ", PriorityLevel.High);

        item.Title.Should().Be("Comprar entradas");
        item.Description.Should().Be("Para el evento");
        item.Priority.Should().Be(PriorityLevel.High);
        item.IsCompleted.Should().BeFalse();
        item.Id.Should().NotBe(Guid.Empty);
        item.DomainEvents.Should().ContainSingle(e => e is TodoItemCreatedEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_ConTituloVacio_LanzaDomainException(string? title)
    {
        var act = () => TodoItem.Create(title!);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ConTituloDemasiadoLargo_LanzaDomainException()
    {
        var title = new string('a', 201);

        var act = () => TodoItem.Create(title);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkAsComplete_MarcaComoCompletadaYRegistraEvento()
    {
        var item = TodoItem.Create("Tarea");
        item.ClearDomainEvents();

        item.MarkAsComplete();

        item.IsCompleted.Should().BeTrue();
        item.CompletedAt.Should().NotBeNull();
        item.DomainEvents.Should().ContainSingle(e => e is TodoItemCompletedEvent);
    }

    [Fact]
    public void MarkAsComplete_EsIdempotente()
    {
        var item = TodoItem.Create("Tarea");
        item.MarkAsComplete();
        var firstCompletedAt = item.CompletedAt;
        item.ClearDomainEvents();

        item.MarkAsComplete();

        item.CompletedAt.Should().Be(firstCompletedAt);
        item.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Reopen_RestableceElEstadoDeCompletada()
    {
        var item = TodoItem.Create("Tarea");
        item.MarkAsComplete();

        item.Reopen();

        item.IsCompleted.Should().BeFalse();
        item.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void UpdateDetails_ActualizaLasPropiedadesEditables()
    {
        var item = TodoItem.Create("Original");

        item.UpdateDetails("Nuevo título", "Nueva descripción", PriorityLevel.Medium);

        item.Title.Should().Be("Nuevo título");
        item.Description.Should().Be("Nueva descripción");
        item.Priority.Should().Be(PriorityLevel.Medium);
    }
}
