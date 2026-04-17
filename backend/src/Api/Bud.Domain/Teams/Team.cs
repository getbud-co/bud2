namespace Bud.Domain.Teams;

public sealed class Team : ITenantEntity, IAggregateRoot
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TeamColor Color { get; set; } = TeamColor.Neutral;
    public TeamStatus Status { get; set; } = TeamStatus.Active;
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public Guid? ParentTeamId { get; set; }
    public Team? ParentTeam { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<EmployeeTeam> EmployeeTeams { get; set; } = new List<EmployeeTeam>();

    public Guid? LeaderId => EmployeeTeams.SingleOrDefault(et => et.Role == TeamRole.Leader)?.EmployeeId;

    public static Team Create(Guid id, Guid organizationId, string name, Guid leaderId, Guid? parentTeamId = null, string? description = null, TeamColor color = TeamColor.Neutral)
    {
        if (organizationId == Guid.Empty)
        {
            throw new DomainInvariantException("Time deve pertencer a uma organização válida.");
        }

        if (leaderId == Guid.Empty)
        {
            throw new DomainInvariantException("O líder do time é obrigatório.");
        }

        var now = DateTime.UtcNow;
        var team = new Team
        {
            Id = id,
            OrganizationId = organizationId,
            Description = description,
            Color = color,
            Status = TeamStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        };

        team.Rename(name);
        team.Reparent(parentTeamId, id);

        return team;
    }

    public void AssignLeader(Guid newLeaderId)
    {
        if (newLeaderId == Guid.Empty)
        {
            throw new DomainInvariantException("O líder do time é obrigatório.");
        }

        var currentLeader = EmployeeTeams.SingleOrDefault(et => et.Role == TeamRole.Leader);
        if (currentLeader is not null)
        {
            currentLeader.Role = TeamRole.Member;
        }

        var entry = EmployeeTeams.FirstOrDefault(et => et.EmployeeId == newLeaderId);
        if (entry is not null)
        {
            entry.Role = TeamRole.Leader;
        }
        else
        {
            EmployeeTeams.Add(new EmployeeTeam
            {
                EmployeeId = newLeaderId,
                TeamId = Id,
                Role = TeamRole.Leader,
                AssignedAt = DateTime.UtcNow
            });
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void Rename(string name)
    {
        if (!EntityName.TryCreate(name, out var entityName))
        {
            throw new DomainInvariantException("O nome do time é obrigatório e deve ter até 200 caracteres.");
        }

        Name = entityName.Value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Describe(string? description)
    {
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetColor(TeamColor color)
    {
        Color = color;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStatus(TeamStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reparent(Guid? parentTeamId, Guid selfId)
    {
        if (parentTeamId.HasValue && parentTeamId.Value == selfId)
        {
            throw new DomainInvariantException("Um time não pode ser seu próprio pai.");
        }

        ParentTeamId = parentTeamId;
        UpdatedAt = DateTime.UtcNow;
    }
}
