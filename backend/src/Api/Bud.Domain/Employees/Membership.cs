namespace Bud.Domain.Employees;

/// <summary>
/// Componente secundário do agregado Employee. Representa o vínculo de um colaborador
/// a uma organização específica. Só deve ser acessado via Employee.Memberships.
/// </summary>
public sealed class Membership : ITenantEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public EmployeeRole Role { get; set; } = EmployeeRole.Contributor;
    public Guid? LeaderId { get; set; }

    // TODO: Remove GlobalAdmin Flag and replace for a specific Role, with stronger requirements
    public bool IsGlobalAdmin { get; set; }

    public static Membership Create(
        Guid employeeId,
        Guid organizationId,
        EmployeeRole role,
        Guid? leaderId = null)
    {
        if (organizationId == Guid.Empty)
        {
            throw new DomainInvariantException("Colaborador deve pertencer a uma organização válida.");
        }

        return new Membership
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
