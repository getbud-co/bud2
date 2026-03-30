using FluentAssertions;
using Xunit;

namespace Bud.ArchitectureTests.Architecture;

public sealed class AggregateRootArchitectureTests
{
    [Fact]
    public void AggregateRoots_ShouldImplementIAggregateRoot()
    {
        var expectedAggregateRoots = new[]
        {
            typeof(Organization),
            typeof(Team),
            typeof(Employee),
            typeof(Mission),
            typeof(Indicator),
            typeof(Template),
            typeof(Notification)
        };

        var missingMarker = expectedAggregateRoots
            .Where(type => !typeof(IAggregateRoot).IsAssignableFrom(type))
            .Select(type => type.FullName)
            .ToList();

        missingMarker.Should().BeEmpty("aggregate roots devem ser explícitas para reforçar boundaries de domínio");
    }

    [Fact]
    public void ChildEntities_ShouldNotImplementIAggregateRoot()
    {
        var nonRoots = new[]
        {
            typeof(Checkin),
            typeof(TemplateMission),
            typeof(TemplateIndicator),
            typeof(EmployeeTeam),
            typeof(EmployeeAccessLog)
        };

        var invalidRoots = nonRoots
            .Where(type => typeof(IAggregateRoot).IsAssignableFrom(type))
            .Select(type => type.FullName)
            .ToList();

        invalidRoots.Should().BeEmpty("entidades internas não devem ser marcadas como aggregate roots");
    }
}
