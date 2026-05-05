using System.ComponentModel.DataAnnotations.Schema;
using Bud.Domain.Tags;

namespace Bud.Domain.Missions;

public sealed class Mission : ITenantEntity, IAggregateRoot, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; set; }

    // Organization
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    // Cycle

    public Guid? CycleId { get; set; }
    public Cycle? Cycle { get; set; }

    // Ordenation field for kanban
    public string SortOrder { get; set; } = string.Empty;

    // Mission hierarchy
    public Array<string> Path { get; set; } = Array.Empty<string>();

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Dimension { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public MissionStatus Status { get; set; }

    public MissionVisibility Visibility { get; set; }

    public MissionKanbanStatus? KanbanStatus { get; set; } = MissionKanbanStatus.Uncategorized;


    // Recursive parent-child relationship
    public Guid? ParentId { get; set; }
    public Mission? Parent { get; set; }
    public ICollection<Mission> Children { get; set; } = [];

    // Responsible employee (optional)
    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public ICollection<MissionMember> Members { get; set; } = [];
    public ICollection<Indicator> Indicators { get; set; } = [];
    public ICollection<MissionTask> Tasks { get; set; } = [];
    public ICollection<MissionTag> Tags { get; set; } = [];

    // Auditing fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();


    public static Mission Create(
        Guid id,
        Guid organizationId,
        string name,
        string? description,
        string? dimension,
        DateTime startDate,
        DateTime endDate,
        MissionStatus status,
        MissionVisibility visibility,
        Guid? parentId = null,
        Guid? actorEmployeeId = null)
    {
        if (organizationId == Guid.Empty)
        {
            throw new DomainInvariantException("Meta deve pertencer a uma organização válida.");
        }

        var mission = new Mission
        {
            Id = id,
            OrganizationId = organizationId,
            ParentId = parentId
        };

        mission.ApplyDetails(name, description, dimension, startDate, endDate, status, visibility);
        mission.AddDomainEvent(new MissionCreatedDomainEvent(mission.Id, mission.OrganizationId, mission.Title, actorEmployeeId));
        return mission;
    }

    public void UpdateDetails(
        string name,
        string? description,
        string? dimension,
        DateTime startDate,
        DateTime endDate,
        MissionStatus status,
        MissionVisibility visibility)
    {
        ApplyDetails(name, description, dimension, startDate, endDate, status, visibility);
    }

    public void MarkAsUpdated(Guid? actorEmployeeId = null)
    {
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new MissionUpdatedDomainEvent(Id, OrganizationId, Title, actorEmployeeId));
    }

    public void MarkAsDeleted(Guid? actorEmployeeId = null)
    {
        DeletedAt = DateTime.UtcNow;
        AddDomainEvent(new MissionDeletedDomainEvent(Id, OrganizationId, Title, actorEmployeeId));
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private void ApplyDetails(
        string name,
        string? description,
        string? dimension,
        DateTime startDate,
        DateTime endDate,
        MissionStatus status,
        MissionVisibility visibility)
    {
        if (!EntityName.TryCreate(name, out var entityName))
        {
            throw new DomainInvariantException("O nome da meta é obrigatório e deve ter até 200 caracteres.");
        }

        if (endDate < startDate)
        {
            throw new DomainInvariantException("Data de término deve ser igual ou posterior à data de início.");
        }

        Title = entityName.Value;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Dimension = string.IsNullOrWhiteSpace(dimension) ? null : dimension.Trim();
        StartDate = startDate;
        EndDate = endDate;
        Status = status;
        Visibility = visibility;
    }

    public string BuildLtreePath()
    {
        var segments = new List<string>();
        var current = this;
        while (current is not null)
        {
            segments.Add(current.Id.ToString("N"));
            current = current.Parent;
        }
        segments.Reverse();
        return string.Join('.', segments);
    }

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
