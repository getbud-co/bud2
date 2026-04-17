using FluentAssertions;
using Xunit;

namespace Bud.Api.UnitTests.Domain.ValueObjects;

public sealed class EmployeeNameTests
{
    [Fact]
    public void Create_WithValidName_NormalizesValue()
    {
        var employeeName = EmployeeName.Create("  Maria   da   Silva  ");

        employeeName.Value.Should().Be("Maria da Silva");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void Create_WithInvalidName_ThrowsDomainInvariantException(string raw)
    {
        var act = () => EmployeeName.Create(raw);

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("O nome do colaborador é obrigatório.");
    }
}
