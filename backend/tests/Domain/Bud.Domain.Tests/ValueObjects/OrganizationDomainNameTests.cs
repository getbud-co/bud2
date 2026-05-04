namespace Bud.Domain.Tests.ValueObjects;

public sealed class OrganizationDomainNameTests
{
    [Theory]
    [InlineData("empresa.com.br")]
    [InlineData("my-company.org")]
    [InlineData("sub.domain.co.uk")]
    [InlineData("getbud.co")]
    [InlineData("example.com")]
    public void TryCreate_WithValidDomain_ShouldSucceed(string raw)
    {
        OrganizationDomainName.TryCreate(raw, out var name).Should().BeTrue();
        name.Value.Should().Be(raw);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("just text")]
    [InlineData("no-dots")]
    [InlineData("-invalid.com")]
    [InlineData("invalid-.com")]
    [InlineData(".leading-dot.com")]
    [InlineData("spaces in.com")]
    [InlineData("under_score.com")]
    public void TryCreate_WithInvalidDomain_ShouldFail(string? raw)
    {
        OrganizationDomainName.TryCreate(raw, out _).Should().BeFalse();
    }

    [Fact]
    public void TryCreate_WithExceedingMaxLength_ShouldFail()
    {
        var longLabel = new string('a', 197);
        var domain = $"{longLabel}.com"; // 201 chars
        OrganizationDomainName.TryCreate(domain, out _).Should().BeFalse();
    }

    [Fact]
    public void TryCreate_ShouldTrimWhitespace()
    {
        OrganizationDomainName.TryCreate("  empresa.com  ", out var name).Should().BeTrue();
        name.Value.Should().Be("empresa.com");
    }

    [Fact]
    public void Create_WithValidDomain_ShouldReturnValue()
    {
        var name = OrganizationDomainName.Create("getbud.co");
        name.Value.Should().Be("getbud.co");
    }

    [Fact]
    public void Create_WithInvalidDomain_ShouldThrowDomainInvariantException()
    {
        var act = () => OrganizationDomainName.Create("not a domain");
        act.Should().Throw<DomainInvariantException>();
    }
}
