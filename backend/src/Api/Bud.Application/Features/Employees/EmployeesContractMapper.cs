
namespace Bud.Application.Features.Employees;

public static class EmployeesContractMapper
{
    public static EmployeeResponse ToEmployeeResponse(this Employee source)
    {
        return new EmployeeResponse
        {
            Id = source.Id,
            FullName = source.FullName,
            Email = source.Email,
            Role = source.Role,
            OrganizationId = source.OrganizationId,
            TeamId = source.TeamId,
            LeaderId = source.LeaderId,
            IsGlobalAdmin = source.IsGlobalAdmin
        };
    }

    public static EmployeeLookupResponse ToResponse(this Employee source)
    {
        return new EmployeeLookupResponse
        {
            Id = source.Id,
            FullName = source.FullName,
            Email = source.Email,
            Role = source.Role        };
    }

    public static EmployeeTeamResponse ToEmployeeTeamResponse(this Team source)
    {
        return new EmployeeTeamResponse
        {
            Id = source.Id,
            Name = source.Name
        };
    }

    public static EmployeeTeamEligibleResponse ToEmployeeTeamEligibleResponse(this Team source)
    {
        return new EmployeeTeamEligibleResponse
        {
            Id = source.Id,
            Name = source.Name
        };
    }

    public static TeamEmployeeEligibleResponse ToTeamEmployeeEligibleResponse(this Employee source)
    {
        return new TeamEmployeeEligibleResponse
        {
            Id = source.Id,
            FullName = source.FullName,
            Email = source.Email,
            Role = source.Role        };
    }

    public static EmployeeLeaderResponse ToLeaderResponse(this Employee source)
    {
        return new EmployeeLeaderResponse
        {
            Id = source.Id,
            FullName = source.FullName,
            Email = source.Email,
            TeamName = source.Team?.Name,
            OrganizationName = source.Organization?.Name ?? string.Empty
        };
    }
}
