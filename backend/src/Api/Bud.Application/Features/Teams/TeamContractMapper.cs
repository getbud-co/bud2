using Bud.Application.Features.Employees;

namespace Bud.Application.Features.Teams;

public static class TeamContractMapper
{
    public static TeamResponse ToResponse(this Team source)
    {
        return new TeamResponse
        {
            Id = source.Id,
            Name = source.Name,
            OrganizationId = source.OrganizationId,
            ParentTeamId = source.ParentTeamId,
            LeaderId = source.LeaderId,
            ParentTeam = source.ParentTeam?.ToResponse(),
            SubTeams = source.SubTeams.Select(t => t.ToResponse()).ToList(),
            Employees = source.Employees.Select(c => c.ToEmployeeResponse()).ToList(),
            Leader = source.Leader?.ToEmployeeResponse()
        };
    }
}
