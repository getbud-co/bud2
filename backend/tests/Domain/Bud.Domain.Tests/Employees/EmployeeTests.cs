namespace Bud.Domain.Tests.Employees;

public sealed class EmployeeTests
{
    private static readonly EmployeeName ValidName = EmployeeName.Create("João Silva");
    private static readonly EmailAddress ValidEmail = EmailAddress.Create("joao@bud.co");

    [Fact]
    public void Create_WithValidData_ShouldInitializeCorrectly()
    {
        var orgId = Guid.NewGuid();
        var id = Guid.NewGuid();

        var employee = Employee.Create(id, orgId, ValidName, ValidEmail, EmployeeRole.Leader);

        employee.Id.Should().Be(id);
        employee.OrganizationId.Should().Be(orgId);
        employee.FullName.Should().Be(ValidName);
        employee.Email.Should().Be(ValidEmail);
        employee.Role.Should().Be(EmployeeRole.Leader);
    }

    [Fact]
    public void Create_WithEmptyOrganizationId_ShouldThrow()
    {
        var act = () => Employee.Create(Guid.NewGuid(), Guid.Empty, ValidName, ValidEmail, EmployeeRole.IndividualContributor);

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("*organização*");
    }

    [Fact]
    public void UpdateProfile_ShouldUpdateAllFields()
    {
        var employee = Employee.Create(Guid.NewGuid(), Guid.NewGuid(), ValidName, ValidEmail, EmployeeRole.IndividualContributor);
        var newName = EmployeeName.Create("Maria Santos");
        var newEmail = EmailAddress.Create("maria@bud.co");

        employee.UpdateProfile(newName, newEmail, EmployeeRole.Leader);

        employee.FullName.Should().Be(newName);
        employee.Email.Should().Be(newEmail);
        employee.Role.Should().Be(EmployeeRole.Leader);
    }
}
