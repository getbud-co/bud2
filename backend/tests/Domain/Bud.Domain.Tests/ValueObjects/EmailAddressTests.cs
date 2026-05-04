namespace Bud.Domain.Tests.ValueObjects;

public sealed class EmailAddressTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("USER@EXAMPLE.COM")]
    [InlineData("a@b.co")]
    public void TryCreate_WithValidEmail_ShouldSucceedAndNormalize(string raw)
    {
        EmailAddress.TryCreate(raw, out var email).Should().BeTrue();
        email.Value.Should().Be(raw.Trim().ToLowerInvariant());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("no-at-sign")]
    [InlineData("@missing-local.com")]
    [InlineData("double@@at.com")]
    [InlineData("user@")]
    [InlineData("user@no-dot")]
    public void TryCreate_WithInvalidEmail_ShouldFail(string? raw)
    {
        EmailAddress.TryCreate(raw, out _).Should().BeFalse();
    }

    [Fact]
    public void Create_WithValidEmail_ShouldReturnValue()
    {
        var email = EmailAddress.Create("Admin@Bud.Co");
        email.Value.Should().Be("admin@bud.co");
    }

    [Fact]
    public void Create_WithInvalidEmail_ShouldThrowDomainInvariantException()
    {
        var act = () => EmailAddress.Create("invalid");
        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void TryCreate_ShouldTrimWhitespace()
    {
        EmailAddress.TryCreate("  user@example.com  ", out var email).Should().BeTrue();
        email.Value.Should().Be("user@example.com");
    }
}
