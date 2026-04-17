using FluentAssertions;
using Xunit;

namespace Bud.Api.UnitTests.Domain.ValueObjects;

public sealed class OrganizationDomainNameTests
{
    [Fact]
    public void Create_WithValidDomain_NormalizesValue()
    {
        var domainName = OrganizationDomainName.Create("  Empresa.COM.BR ");

        domainName.Value.Should().Be("empresa.com.br");
    }

    [Theory]
    [InlineData("under_score.com")]
    [InlineData("invalid")]
    [InlineData("")]
    public void Create_WithInvalidDomain_ThrowsDomainInvariantException(string raw)
    {
        var act = () => OrganizationDomainName.Create(raw);

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("O nome da organização deve ser um domínio válido (ex: empresa.com.br).");
    }
}
