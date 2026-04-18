namespace Bud.Domain.Tests.ValueObjects;

public sealed class EmployeeNameTests
{
    [Theory]
    [InlineData("João Silva", "João Silva")]
    [InlineData("  Maria  Santos  ", "Maria Santos")]
    [InlineData("AB", "AB")]
    public void TryCreate_WithValidName_ShouldSucceedAndNormalize(string raw, string expected)
    {
        EmployeeName.TryCreate(raw, out var name).Should().BeTrue();
        name.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void TryCreate_WithInvalidName_ShouldFail(string? raw)
    {
        EmployeeName.TryCreate(raw, out _).Should().BeFalse();
    }

    [Fact]
    public void TryCreate_WithExceedingMaxLength_ShouldFail()
    {
        var longName = new string('a', 201);
        EmployeeName.TryCreate(longName, out _).Should().BeFalse();
    }

    [Fact]
    public void Create_WithValidName_ShouldReturnValue()
    {
        var name = EmployeeName.Create("  João   Silva  ");
        name.Value.Should().Be("João Silva");
    }

    [Fact]
    public void Create_WithInvalidName_ShouldThrowDomainInvariantException()
    {
        var act = () => EmployeeName.Create("");
        act.Should().Throw<DomainInvariantException>();
    }
}
