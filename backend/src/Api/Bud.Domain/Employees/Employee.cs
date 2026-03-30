namespace Bud.Domain.Employees;

public sealed class Employee : ITenantEntity, IAggregateRoot
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public EmployeeRole Role { get; set; } = EmployeeRole.IndividualContributor;
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }
    public Guid? LeaderId { get; set; }
    public Employee? Leader { get; set; }
    public bool IsGlobalAdmin { get; set; }
    public ICollection<EmployeeTeam> EmployeeTeams { get; set; } = new List<EmployeeTeam>();

    public static Employee Create(
        Guid id,
        Guid organizationId,
        string fullName,
        string email,
        EmployeeRole role,
        Guid? leaderId = null)
    {
        if (organizationId == Guid.Empty)
        {
            throw new DomainInvariantException("Colaborador deve pertencer a uma organização válida.");
        }

        var employee = new Employee
        {
            Id = id,
            OrganizationId = organizationId,
            TeamId = null
        };

        employee.UpdateProfile(fullName, email, role, leaderId, id);
        return employee;
    }

    public void UpdateProfile(string fullName, string email, EmployeeRole role, Guid? leaderId, Guid selfId)
    {
        if (!PersonName.TryCreate(fullName, out var personName))
        {
            throw new DomainInvariantException("O nome do colaborador é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainInvariantException("O e-mail do colaborador é obrigatório.");
        }

        if (leaderId.HasValue && leaderId.Value == selfId)
        {
            throw new DomainInvariantException("Um colaborador não pode ser líder de si mesmo.");
        }

        FullName = personName.Value;
        Email = email.Trim();
        Role = role;
        LeaderId = leaderId;
    }

    public void EnsureCanLeadOrganization(Guid organizationId)
    {
        if (Role != EmployeeRole.Leader)
        {
            throw new DomainInvariantException("O colaborador selecionado deve ter o perfil de Líder.");
        }

        if (OrganizationId != organizationId)
        {
            throw new DomainInvariantException("O líder deve pertencer à mesma organização.");
        }
    }
}
