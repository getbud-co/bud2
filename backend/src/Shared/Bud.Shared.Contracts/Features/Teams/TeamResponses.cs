using Bud.Shared.Kernel;

namespace Bud.Shared.Contracts.Features.Teams;

public sealed class TeamResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TeamColor Color { get; set; }
    public TeamStatus Status { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? ParentTeamId { get; set; }
    public Guid? LeaderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public TeamResponse? ParentTeam { get; set; }
    public List<EmployeeMembershipResponse> Employees { get; set; } = [];
    public EmployeeMembershipResponse? Leader { get; set; }
}

public sealed class TeamEmployeeEligibleResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public EmployeeRole Role { get; set; }
}
