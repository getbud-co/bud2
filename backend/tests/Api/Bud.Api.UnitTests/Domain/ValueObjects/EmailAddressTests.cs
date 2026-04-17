using FluentAssertions;
using Xunit;

namespace Bud.Api.UnitTests.Domain.ValueObjects;

public sealed class EmailAddressTests
{
    [Fact]
    public void Create_WithValidEmail_NormalizesValue()
    {
        var emailAddress = EmailAddress.Create("  USER@Example.COM ");

        emailAddress.Value.Should().Be("user@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid")]
    [InlineData("missing-at-sign")]
    public void Create_WithInvalidEmail_ThrowsDomainInvariantException(string raw)
    {
        var act = () => EmailAddress.Create(raw);

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("O e-mail informado é inválido.");
    }
}
