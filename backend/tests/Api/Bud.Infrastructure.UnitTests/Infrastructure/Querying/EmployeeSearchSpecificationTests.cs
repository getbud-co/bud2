using Bud.Infrastructure.Querying;
using FluentAssertions;
using Xunit;

namespace Bud.Infrastructure.UnitTests.Infrastructure.Querying;

public sealed class EmployeeSearchSpecificationTests
{
    [Fact]
    public void Apply_WithSearchTerm_ShouldFilterByNameOrEmail()
    {
        var data = new List<Employee>
        {
            new() { Id = Guid.NewGuid(), FullName = "Ana Silva", Email = "ana@example.com" },
            new() { Id = Guid.NewGuid(), FullName = "Bruno Costa", Email = "bruno@example.com" }
        }.AsQueryable();

        var specification = new EmployeeSearchSpecification("ANA", isNpgsql: false);

        var result = specification.Apply(data).ToList();

        result.Should().HaveCount(1);
        result[0].FullName.Should().Be("Ana Silva");
    }
}
