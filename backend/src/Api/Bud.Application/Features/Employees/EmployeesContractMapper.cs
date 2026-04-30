namespace Bud.Application.Features.Employees;

public static class EmployeesContractMapper
{
    public static EmployeeResponse ToEmployeeResponse(this Employee employee)
    {
        return new EmployeeResponse
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Email = employee.Email,
            Nickname = employee.Nickname,
            Language = employee.Language,
            Status = employee.Status,
        };
    }

    /// <summary>
    /// Mapeia Employee para EmployeeMembershipResponse lendo o vínculo organizacional
    /// via Employee.GetMembership() (coleção filtrada por tenant — sempre um item).
    /// </summary>
    public static EmployeeMembershipResponse ToEmployeeMembershipResponse(this Employee employee)
    {
        var membership = employee.GetMembership();
        return new EmployeeMembershipResponse
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Email = employee.Email,
            Nickname = employee.Nickname,
            Language = employee.Language,
            Status = employee.Status,
            Role = membership?.Role ?? EmployeeRole.Contributor,
            OrganizationId = membership?.OrganizationId ?? Guid.Empty,
            LeaderId = membership?.LeaderId,
            IsGlobalAdmin = membership?.IsGlobalAdmin ?? false,
            Teams = employee.EmployeeTeams
                .Select(et => new TeamResponse { Id = et.Team.Id, Name = et.Team.Name })
                .ToList(),
        };
    }

    public static EmployeeLookupResponse ToLookupResponse(this Employee employee)
    {
        return new EmployeeLookupResponse
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Email = employee.Email,
            Role = employee.GetMembership()?.Role ?? EmployeeRole.Contributor,
        };
    }

    public static EmployeeLeaderResponse ToLeaderResponse(this Employee employee)
    {
        var membership = employee.GetMembership();
        return new EmployeeLeaderResponse
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Email = employee.Email,
            TeamName = employee.EmployeeTeams.FirstOrDefault()?.Team?.Name,
            OrganizationName = membership?.Organization?.Name ?? string.Empty,
        };
    }

    public static TeamEmployeeEligibleResponse ToTeamEmployeeEligibleResponse(this Employee employee)
    {
        return new TeamEmployeeEligibleResponse
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Email = employee.Email,
            Role = employee.GetMembership()?.Role ?? EmployeeRole.Contributor,
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
}
