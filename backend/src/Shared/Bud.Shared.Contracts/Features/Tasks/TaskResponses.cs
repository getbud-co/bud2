using Bud.Shared.Kernel;

namespace Bud.Shared.Contracts.Features.Tasks;

public sealed class TaskResponse
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid MissionId { get; set; }
    public Guid EmployeeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDone { get; set; }
    public DateOnly? DueDate { get; set; }
    public string SortOrder { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
