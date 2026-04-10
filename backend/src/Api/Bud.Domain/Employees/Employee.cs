namespace Bud.Domain.Employees;

public sealed class Employee : IAggregateRoot
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public EmployeeLanguage Language { get; set; } = EmployeeLanguage.Pt;
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Invited;

    public ICollection<EmployeeTeam> EmployeeTeams { get; set; } = new List<EmployeeTeam>();
    public ICollection<OrganizationEmployeeMember> Memberships { get; set; } = new List<OrganizationEmployeeMember>();

    public static Employee Create(Guid id, string fullName, string email)
    {
        var employee = new Employee { Id = id };
        employee.UpdateIdentity(fullName, email);
        return employee;
    }

    public void UpdateIdentity(string fullName, string email)
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
    }
}
