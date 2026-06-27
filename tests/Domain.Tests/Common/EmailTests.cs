using Ceiba.LiveEvent.Reservations.Domain.Common;
using Ceiba.LiveEvent.Reservations.Domain.Exceptions;
using FluentAssertions;

namespace Ceiba.LiveEvent.Reservations.Domain.Tests.Common;

public class EmailTests
{
    [Theory]
    [InlineData("ana@example.com")]
    [InlineData("  ANA@EXAMPLE.COM  ")]
    [InlineData("a.b+c@sub.dominio.co")]
    public void Create_ConFormatoValido_NormalizaYAcepta(string input)
    {
        var email = Email.Create(input);

        email.Value.Should().Be(input.Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("sin-arroba")]
    [InlineData("a@b")]
    [InlineData("a@@b.com")]
    [InlineData("espacio @b.com")]
    public void Create_ConFormatoInvalido_LanzaDomainException(string input)
    {
        var act = () => Email.Create(input);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Equals_EsCaseInsensitive()
    {
        Email.Create("ana@example.com").Should().Be(Email.Create("ANA@EXAMPLE.COM"));
    }
}
