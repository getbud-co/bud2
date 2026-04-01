using System.ComponentModel.DataAnnotations.Schema;

namespace Bud.Domain.Missions;

public sealed class Mission : ITenantEntity, IAggregateRoot, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; set; }

    // Organization
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    // Cycle

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Dimension { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public MissionStatus Status { get; set; }


    // Recursive parent-child relationship
    public Guid? ParentId { get; set; }
    public Mission? Parent { get; set; }
    public ICollection<Mission> Children { get; set; } = [];

    // Responsible employee (optional)
    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public ICollection<Indicator> Indicators { get; set; } = [];
    public ICollection<MissionTask> Tasks { get; set; } = [];

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

        mission.ApplyDetails(name, description, dimension, startDate, endDate, status);
        mission.AddDomainEvent(new MissionCreatedDomainEvent(mission.Id, mission.OrganizationId, mission.Name, actorEmployeeId));
        return mission;
    }

    public void UpdateDetails(
        string name,
        string? description,
        string? dimension,
        DateTime startDate,
        DateTime endDate,
        MissionStatus status)
    {
        ApplyDetails(name, description, dimension, startDate, endDate, status);
    }

    public void MarkAsUpdated(Guid? actorEmployeeId = null)
    {
        AddDomainEvent(new MissionUpdatedDomainEvent(Id, OrganizationId, Name, actorEmployeeId));
    }

    public void MarkAsDeleted(Guid? actorEmployeeId = null)
    {
        AddDomainEvent(new MissionDeletedDomainEvent(Id, OrganizationId, Name, actorEmployeeId));
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
        MissionStatus status)
    {
        if (!EntityName.TryCreate(name, out var entityName))
        {
            throw new DomainInvariantException("O nome da meta é obrigatório e deve ter até 200 caracteres.");
        }

        if (endDate < startDate)
        {
            throw new DomainInvariantException("Data de término deve ser igual ou posterior à data de início.");
        }

        Name = entityName.Value;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Dimension = string.IsNullOrWhiteSpace(dimension) ? null : dimension.Trim();
        StartDate = startDate;
        EndDate = endDate;
        Status = status;
    }

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
