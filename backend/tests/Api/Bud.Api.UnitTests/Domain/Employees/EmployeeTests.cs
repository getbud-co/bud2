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
            EmployeeName.Create("  Maria   da   Silva  "),
            EmailAddress.Create("  MARIA@Example.COM "),
            EmployeeRole.Leader);

        employee.FullName.Value.Should().Be("Maria da Silva");
        employee.Email.Value.Should().Be("maria@example.com");
    }

    [Fact]
    public void UpdateProfile_WithValidName_UpdatesCanonicalValues()
    {
        var employee = Employee.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EmployeeName.Create("Maria Silva"),
            EmailAddress.Create("maria@example.com"),
            EmployeeRole.Leader);

        employee.UpdateProfile(
            EmployeeName.Create("  Maria   Souza  "),
            EmailAddress.Create("  MARIA@bud.com "),
            EmployeeRole.IndividualContributor);

        employee.FullName.Value.Should().Be("Maria Souza");
        employee.Email.Value.Should().Be("maria@bud.com");
        employee.Role.Should().Be(EmployeeRole.IndividualContributor);
    }
}
