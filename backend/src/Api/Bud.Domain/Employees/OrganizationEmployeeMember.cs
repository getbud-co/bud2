namespace Bud.Domain.Employees;

public sealed class OrganizationEmployeeMember : IAggregateRoot, ITenantEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public EmployeeRole Role { get; set; } = EmployeeRole.Contributor;
    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }
    public Guid? LeaderId { get; set; }
    public Employee? Leader { get; set; }
    public bool IsGlobalAdmin { get; set; }

    public static OrganizationEmployeeMember Create(
        Guid employeeId,
        Guid organizationId,
        EmployeeRole role,
        Guid? leaderId = null)
    {
        if (organizationId == Guid.Empty)
        {
            throw new DomainInvariantException("Colaborador deve pertencer a uma organização válida.");
        }

        return new OrganizationEmployeeMember
        {
            EmployeeId = employeeId,
            OrganizationId = organizationId,
            Role = role,
            LeaderId = leaderId,
        };
    }

    public void UpdateProfile(EmployeeRole role, Guid? leaderId, Guid selfId)
    {
        if (leaderId.HasValue && leaderId.Value == selfId)
        {
            throw new DomainInvariantException("Um colaborador não pode ser líder de si mesmo.");
        }

        Role = role;
        LeaderId = leaderId;
    }

    public void EnsureCanLeadOrganization(Guid organizationId)
    {
        if (Role != EmployeeRole.TeamLeader)
        {
            throw new DomainInvariantException("O colaborador selecionado deve ter o perfil de Líder.");
        }

        if (OrganizationId != organizationId)
        {
            throw new DomainInvariantException("O líder deve pertencer à mesma organização.");
        }
    }
}
