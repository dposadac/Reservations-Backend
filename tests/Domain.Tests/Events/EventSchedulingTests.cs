using Ceiba.LiveEvent.Reservations.Domain.Events;
using Ceiba.LiveEvent.Reservations.Domain.Exceptions;
using FluentAssertions;

namespace Ceiba.LiveEvent.Reservations.Domain.Tests.Events;

public class EventSchedulingTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly Guid VenueId = Guid.NewGuid();
    private static readonly Guid TypeId = Guid.NewGuid();
    private static readonly Guid ActiveId = Guid.NewGuid();

    private static Event Existing(int startDays)
        => Event.Create("Evento", "Descripción del evento.", VenueId, 1000, 100,
            Now.AddDays(startDays), Now.AddDays(startDays).AddHours(2), 10m, TypeId, ActiveId, Now);

    [Fact] // RN-02
    public void EnsureNoVenueOverlap_SinSolape_NoLanza()
    {
        var existing = new[] { Existing(10) };

        var act = () => EventScheduling.EnsureNoVenueOverlap(
            Now.AddDays(12), Now.AddDays(12).AddHours(2), existing);

        act.Should().NotThrow();
    }

    [Fact] // RN-02
    public void EnsureNoVenueOverlap_ConSolape_LanzaBusinessRuleException()
    {
        var existing = new[] { Existing(10) }; // [día10 00:00, día10 02:00)

        var act = () => EventScheduling.EnsureNoVenueOverlap(
            Now.AddDays(10).AddHours(1), Now.AddDays(10).AddHours(3), existing);

        act.Should().Throw<BusinessRuleException>()
            .Which.MessageKey.Should().Be("rules.rn02VenueOverlap");
    }
}
