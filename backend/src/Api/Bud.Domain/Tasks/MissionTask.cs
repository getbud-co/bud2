namespace Bud.Domain.Tasks;

public sealed class MissionTask : ITenantEntity, IAggregateRoot
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public Guid MissionId { get; set; }
    public Mission Mission { get; set; } = null!;

    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDone { get; set; }
    public DateOnly? DueDate { get; set; }
    public string SortOrder { get; set; } = string.Empty;

    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static MissionTask Create(
        Guid id,
        Guid organizationId,
        Guid missionId,
        Guid employeeId,
        string title,
        string sortOrder,
        string? description = null,
        DateOnly? dueDate = null)
    {
        if (organizationId == Guid.Empty)
        {
            throw new DomainInvariantException("Tarefa deve pertencer a uma organização válida.");
        }

        if (missionId == Guid.Empty)
        {
            throw new DomainInvariantException("Tarefa deve pertencer a uma meta válida.");
        }

        if (!EntityName.TryCreate(title, out var entityName))
        {
            throw new DomainInvariantException("O título da tarefa é obrigatório e deve ter até 200 caracteres.");
        }

        return new MissionTask
        {
            Id = id,
            OrganizationId = organizationId,
            MissionId = missionId,
            EmployeeId = employeeId,
            Title = entityName.Value,
            SortOrder = sortOrder,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            DueDate = dueDate,
            IsDone = false,
        };
    }

    public void UpdateDetails(string title, string? description, DateOnly? dueDate)
    {
        if (!EntityName.TryCreate(title, out var entityName))
        {
            throw new DomainInvariantException("O título da tarefa é obrigatório e deve ter até 200 caracteres.");
        }

        Title = entityName.Value;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        IsDone = true;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reopen()
    {
        IsDone = false;
        CompletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
