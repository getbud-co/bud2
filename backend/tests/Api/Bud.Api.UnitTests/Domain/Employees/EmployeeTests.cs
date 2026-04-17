using FluentAssertions;
using Xunit;

namespace Bud.Api.UnitTests.Domain.Employees;

public sealed class EmployeeTests
{
    [Fact]
    public void Create_WithValidProfile_NormalizesNameAndEmail()
    {
        var employee = Employee.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "  Maria   da   Silva  ",
            "  MARIA@Example.COM ",
            EmployeeRole.Leader);

        employee.FullName.Should().Be("Maria da Silva");
        employee.Email.Should().Be("maria@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("A")]
    public void UpdateProfile_WithInvalidName_ThrowsDomainInvariantException(string fullName)
    {
        var employee = Employee.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Maria Silva",
            "maria@example.com",
            EmployeeRole.Leader);

        var act = () => employee.UpdateProfile(fullName, "maria@example.com", EmployeeRole.Leader);

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("O nome do colaborador é obrigatório.");
    }

    [Fact]
    public void UpdateProfile_WithInvalidEmail_ThrowsDomainInvariantException()
    {
        var employee = Employee.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Maria Silva",
            "maria@example.com",
            EmployeeRole.Leader);

        var act = () => employee.UpdateProfile("Maria Silva", "invalid", EmployeeRole.Leader);

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("O e-mail informado é inválido.");
    }
}
