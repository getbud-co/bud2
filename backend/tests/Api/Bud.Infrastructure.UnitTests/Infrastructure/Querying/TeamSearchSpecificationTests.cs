using Bud.Infrastructure.Querying;
using FluentAssertions;
using Xunit;

namespace Bud.Infrastructure.UnitTests.Infrastructure.Querying;

public sealed class TeamSearchSpecificationTests
{
    [Fact]
    public void Apply_WithSearchTerm_ShouldFilterByTeamName()
    {
        var organizationId = Guid.NewGuid();
        var data = new List<Team>
        {
            new() { Id = Guid.NewGuid(), Name = "Plataforma", OrganizationId = organizationId },
            new() { Id = Guid.NewGuid(), Name = "Vendas", OrganizationId = organizationId }
        }.AsQueryable();

        var specification = new TeamSearchSpecification("plata", isNpgsql: false);

        var result = specification.Apply(data).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Plataforma");
    }
}
