using Bud.Server.Domain.Model;
using Bud.Server.Infrastructure.Querying;
using FluentAssertions;
using Xunit;

namespace Bud.Server.Tests.Application.Common.Specifications;

public sealed class MetricSearchSpecificationTests
{
    private static readonly Guid MissionId = Guid.NewGuid();

    private static readonly List<Metric> Data =
    [
        new() { Id = Guid.NewGuid(), Name = "Receita Mensal", MissionId = MissionId },
        new() { Id = Guid.NewGuid(), Name = "NPS Score", MissionId = MissionId },
        new() { Id = Guid.NewGuid(), Name = "Taxa de Conversão", MissionId = MissionId }
    ];

    [Fact]
    public void Apply_WithSearchTerm_ShouldFilterByName()
    {
        var spec = new MetricSearchSpecification("receita", isNpgsql: false);

        var result = spec.Apply(Data.AsQueryable()).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Receita Mensal");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Apply_WithNullOrEmptySearch_ShouldReturnAll(string? search)
    {
        var spec = new MetricSearchSpecification(search, isNpgsql: false);

        var result = spec.Apply(Data.AsQueryable()).ToList();

        result.Should().HaveCount(3);
    }

    [Fact]
    public void Apply_WithNoMatch_ShouldReturnEmpty()
    {
        var spec = new MetricSearchSpecification("inexistente", isNpgsql: false);

        var result = spec.Apply(Data.AsQueryable()).ToList();

        result.Should().BeEmpty();
    }
}
