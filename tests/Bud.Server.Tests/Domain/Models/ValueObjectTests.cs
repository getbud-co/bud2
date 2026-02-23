using FluentAssertions;
using Xunit;

namespace Bud.Server.Tests.Domain.Models;

public sealed class PerformanceIndicatorTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(50, -10)]
    [InlineData(100, 100)]
    [InlineData(100, -100)]
    public void Create_WithValidValues_ShouldSucceed(int percentage, int delta)
    {
        var indicator = PerformanceIndicator.Create(percentage, delta);

        indicator.Percentage.Should().Be(percentage);
        indicator.DeltaPercentage.Should().Be(delta);
        indicator.IsPlaceholder.Should().BeFalse();
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(101, 0)]
    public void Create_WithInvalidPercentage_ShouldThrow(int percentage, int delta)
    {
        var act = () => PerformanceIndicator.Create(percentage, delta);

        act.Should().Throw<DomainInvariantException>();
    }

    [Theory]
    [InlineData(50, -101)]
    [InlineData(50, 101)]
    public void Create_WithInvalidDelta_ShouldThrow(int percentage, int delta)
    {
        var act = () => PerformanceIndicator.Create(percentage, delta);

        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void Create_WithIsPlaceholder_ShouldSetFlag()
    {
        var indicator = PerformanceIndicator.Create(0, 0, isPlaceholder: true);

        indicator.IsPlaceholder.Should().BeTrue();
    }

    [Fact]
    public void Zero_ShouldReturnZeroValues()
    {
        var indicator = PerformanceIndicator.Zero();

        indicator.Percentage.Should().Be(0);
        indicator.DeltaPercentage.Should().Be(0);
        indicator.IsPlaceholder.Should().BeFalse();
    }

    [Fact]
    public void Placeholder_ShouldReturnPlaceholder()
    {
        var indicator = PerformanceIndicator.Placeholder();

        indicator.Percentage.Should().Be(0);
        indicator.DeltaPercentage.Should().Be(0);
        indicator.IsPlaceholder.Should().BeTrue();
    }
}

public sealed class EngagementScoreTests
{
    [Theory]
    [InlineData(70, "high")]
    [InlineData(85, "high")]
    [InlineData(100, "high")]
    [InlineData(40, "medium")]
    [InlineData(69, "medium")]
    [InlineData(0, "low")]
    [InlineData(39, "low")]
    public void Create_WithValidScore_ShouldDeriveLevel(int score, string expectedLevel)
    {
        var engagement = EngagementScore.Create(score);

        engagement.Score.Should().Be(score);
        engagement.Level.Should().Be(expectedLevel);
    }

    [Fact]
    public void Create_HighScore_ShouldDeriveHighTip()
    {
        var engagement = EngagementScore.Create(80);

        engagement.Tip.Should().Contain("Excelente");
    }

    [Fact]
    public void Create_MediumScore_ShouldDeriveMediumTip()
    {
        var engagement = EngagementScore.Create(50);

        engagement.Tip.Should().Contain("Bom progresso");
    }

    [Fact]
    public void Create_LowScore_ShouldDeriveLowTip()
    {
        var engagement = EngagementScore.Create(20);

        engagement.Tip.Should().Contain("Atenção");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Create_WithInvalidScore_ShouldThrow(int score)
    {
        var act = () => EngagementScore.Create(score);

        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void Zero_ShouldReturnLowEngagement()
    {
        var engagement = EngagementScore.Zero();

        engagement.Score.Should().Be(0);
        engagement.Level.Should().Be("low");
        engagement.Tip.Should().Contain("Atenção");
    }
}
