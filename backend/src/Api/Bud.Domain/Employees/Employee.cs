namespace Bud.Domain.Employees;

public sealed class Employee : ITenantEntity, IAggregateRoot
{
    public Guid Id { get; set; }
    public EmployeeName FullName { get; private set; }
    public EmailAddress Email { get; private set; }
    public EmployeeRole Role { get; set; } = EmployeeRole.IndividualContributor;
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public bool IsGlobalAdmin { get; set; }

    public static Employee Create(
        Guid id,
        Guid organizationId,
        EmployeeName fullName,
        EmailAddress email,
        EmployeeRole role)
    {
        if (organizationId == Guid.Empty)
        {
            throw new DomainInvariantException("Colaborador deve pertencer a uma organização válida.");
        }

        var employee = new Employee
        {
            Id = id,
            OrganizationId = organizationId
        };

        employee.UpdateProfile(fullName, email, role);
        return employee;
    }

    public void UpdateProfile(EmployeeName fullName, EmailAddress email, EmployeeRole role)
    {
        FullName = fullName;
        Email = email;
        Role = role;
    }
}
