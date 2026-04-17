namespace Bud.Domain.Employees;

public sealed class Employee : ITenantEntity, IAggregateRoot
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public EmployeeRole Role { get; set; } = EmployeeRole.IndividualContributor;
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public bool IsGlobalAdmin { get; set; }

    public static Employee Create(
        Guid id,
        Guid organizationId,
        string fullName,
        string email,
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

    public void UpdateProfile(string fullName, string email, EmployeeRole role)
    {
        if (!PersonName.TryCreate(fullName, out var personName))
        {
            throw new DomainInvariantException("O nome do colaborador é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainInvariantException("O e-mail do colaborador é obrigatório.");
        }

        FullName = personName.Value;
        Email = email.Trim();
        Role = role;
    }
}
