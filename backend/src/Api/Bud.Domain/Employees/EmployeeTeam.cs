namespace Bud.Domain.Employees;

public sealed class EmployeeTeam
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;
    public TeamRole Role { get; set; } = TeamRole.Member;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
