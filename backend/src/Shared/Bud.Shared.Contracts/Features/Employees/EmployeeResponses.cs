using Bud.Shared.Kernel;
using Bud.Shared.Kernel.Enums;

namespace Bud.Shared.Contracts.Features.Employees;

public sealed class EmployeeResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public EmployeeLanguage Language { get; set; } = EmployeeLanguage.Pt;
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Invited;
}

public sealed class EmployeeMembershipResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public EmployeeLanguage Language { get; set; } = EmployeeLanguage.Pt;
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Invited;
    public EmployeeRole Role { get; set; } = EmployeeRole.Contributor;
    public Guid OrganizationId { get; set; }
    public bool IsGlobalAdmin { get; set; }
    public List<TeamResponse> Teams { get; set; } = [];
    public EmployeeMembershipResponse? Leader { get; set; }
}

public sealed class EmployeeLookupResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public EmployeeRole Role { get; set; }
}

public sealed class EmployeeLeaderResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? TeamName { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
}

public sealed class EmployeeSubordinateResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<EmployeeSubordinateResponse> Children { get; set; } = [];
}

public sealed class EmployeeTeamResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class EmployeeTeamEligibleResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
