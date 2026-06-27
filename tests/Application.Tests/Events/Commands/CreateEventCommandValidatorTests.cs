using Ceiba.LiveEvent.Reservations.Application.Events.Commands.CreateEvent;
using Ceiba.LiveEvent.Reservations.Domain.Events;
using FluentAssertions;

namespace Ceiba.LiveEvent.Reservations.Application.Tests.Events.Commands;

public class CreateEventCommandValidatorTests
{
    private readonly CreateEventCommandValidator _validator = new();

    private static CreateEventCommand Valid() => new()
    {
        Title = "Título válido",
        Description = "Descripción suficientemente larga.",
        VenueId = Guid.NewGuid(),
        MaximumCapacity = 100,
        StartDate = DateTimeOffset.UtcNow.AddDays(5),
        EndDate = DateTimeOffset.UtcNow.AddDays(5).AddHours(2),
        TicketPrice = 10m,
        Type = EventType.Taller
    };

    [Fact]
    public void Validate_ConComandoValido_EsValido()
    {
        _validator.Validate(Valid()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ConTituloCorto_Falla()
    {
        var result = _validator.Validate(Valid() with { Title = "abc" });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ConPrecioNoPositivo_Falla()
    {
        _validator.Validate(Valid() with { TicketPrice = 0m }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ConFinAnteriorAlInicio_Falla()
    {
        var start = DateTimeOffset.UtcNow.AddDays(5);
        var result = _validator.Validate(Valid() with { StartDate = start, EndDate = start.AddHours(-1) });
        result.IsValid.Should().BeFalse();
    }
}
