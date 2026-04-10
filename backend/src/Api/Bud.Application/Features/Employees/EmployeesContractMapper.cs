
namespace Bud.Application.Features.Employees;

public static class EmployeesContractMapper
{
    /// <summary>
    /// Maps the identity-only Employee entity (e.g. from nav props on Team/Mission/Checkin).
    /// Org-scoped fields are omitted since they live in OrganizationEmployeeMember.
    /// </summary>
    public static EmployeeResponse ToEmployeeResponse(this Employee employee)
    {
        return new EmployeeResponse
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Email = employee.Email,
            Nickname = employee.Nickname,
            Language = employee.Language,
            Role = EmployeeRole.Contributor,
            OrganizationId = Guid.Empty,
            LeaderId = null,
            IsGlobalAdmin = false,
        };
    }

    public static EmployeeLookupResponse ToResponse(this Employee employee)
    {
        return new EmployeeLookupResponse
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Email = employee.Email,
            Role = EmployeeRole.Contributor,
        };
    }

    public static EmployeeResponse ToEmployeeResponse(this OrganizationEmployeeMember member)
    {
        return new EmployeeResponse
        {
            Id = member.EmployeeId,
            FullName = member.Employee.FullName,
            Email = member.Employee.Email,
            Nickname = member.Employee.Nickname,
            Language = member.Employee.Language,
            Status = member.Employee.Status,
            Role = member.Role,
            OrganizationId = member.OrganizationId,
            LeaderId = member.LeaderId,
            IsGlobalAdmin = member.IsGlobalAdmin,
            Teams = member.Employee.EmployeeTeams
                .Select(et => new TeamResponse { Id = et.Team.Id, Name = et.Team.Name })
                .ToList(),
        };
    }

    public static EmployeeLookupResponse ToResponse(this OrganizationEmployeeMember member)
    {
        return new EmployeeLookupResponse
        {
            Id = member.EmployeeId,
            FullName = member.Employee.FullName,
            Email = member.Employee.Email,
            Role = member.Role,
        };
    }

    public static EmployeeTeamResponse ToEmployeeTeamResponse(this Team source)
    {
        return new EmployeeTeamResponse
        {
            Id = source.Id,
            Name = source.Name,
        };
    }

    public static EmployeeTeamEligibleResponse ToEmployeeTeamEligibleResponse(this Team source)
    {
        return new EmployeeTeamEligibleResponse
        {
            Id = source.Id,
            Name = source.Name,
        };
    }

    public static TeamEmployeeEligibleResponse ToTeamEmployeeEligibleResponse(this OrganizationEmployeeMember member)
    {
        return new TeamEmployeeEligibleResponse
        {
            Id = member.EmployeeId,
            FullName = member.Employee.FullName,
            Email = member.Employee.Email,
            Role = member.Role,
        };
    }

    public static EmployeeLeaderResponse ToLeaderResponse(this OrganizationEmployeeMember member)
    {
        return new EmployeeLeaderResponse
        {
            Id = member.EmployeeId,
            FullName = member.Employee.FullName,
            Email = member.Employee.Email,
            TeamName = member.Team?.Name,
            OrganizationName = member.Organization?.Name ?? string.Empty,
        };
    }
}
