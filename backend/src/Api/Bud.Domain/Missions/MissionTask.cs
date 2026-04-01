
namespace Bud.Domain.Missions;

public sealed class MissionTask : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public Guid MissionId { get; set; }
    public Mission Mission { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskState State { get; set; }
    public DateTime? DueDate { get; private set; }

    public static MissionTask Create(
        Guid id,
        Guid organizationId,
        Guid missionId,
        string name,
        string? description,
        TaskState state,
        DateTime? dueDate = null)
    {
        if (organizationId == Guid.Empty)
        {
            throw new DomainInvariantException("Tarefa deve pertencer a uma organização válida.");
        }

        if (missionId == Guid.Empty)
        {
            throw new DomainInvariantException("Tarefa deve pertencer a uma meta válida.");
        }

        var task = new MissionTask
        {
            Id = id,
            OrganizationId = organizationId,
            MissionId = missionId
        };
        task.UpdateDetails(name, description, state, dueDate);
        return task;
    }

    public void UpdateDetails(string name, string? description, TaskState state, DateTime? dueDate)
    {
        if (!EntityName.TryCreate(name, out var entityName))
        {
            throw new DomainInvariantException("O nome da tarefa é obrigatório e deve ter até 200 caracteres.");
        }

        if (!Enum.IsDefined(state))
        {
            throw new DomainInvariantException("Estado da tarefa inválido.");
        }

        Name = entityName.Value;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        State = state;
        DueDate = dueDate;
    }
}
