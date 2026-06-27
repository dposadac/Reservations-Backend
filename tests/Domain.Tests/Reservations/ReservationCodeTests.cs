using System.Text.RegularExpressions;
using Ceiba.LiveEvent.Reservations.Domain.Exceptions;
using Ceiba.LiveEvent.Reservations.Domain.Reservations;
using FluentAssertions;

namespace Ceiba.LiveEvent.Reservations.Domain.Tests.Reservations;

public class ReservationCodeTests
{
    [Fact]
    public void New_GeneraCodigoConFormatoEV6Digitos()
    {
        var code = ReservationCode.New();

        Regex.IsMatch(code.Value, @"^EV-\d{6}$").Should().BeTrue();
    }

    [Theory]
    [InlineData("EV-123456")]
    public void Create_ConFormatoValido_Acepta(string value)
    {
        ReservationCode.Create(value).Value.Should().Be(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("EV-12345")]   // 5 dígitos
    [InlineData("EV-1234567")] // 7 dígitos
    [InlineData("XX-123456")]
    [InlineData("123456")]
    public void Create_ConFormatoInvalido_LanzaDomainException(string value)
    {
        var act = () => ReservationCode.Create(value);

        act.Should().Throw<DomainException>();
    }
}
