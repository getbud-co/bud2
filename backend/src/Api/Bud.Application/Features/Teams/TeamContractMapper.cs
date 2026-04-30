using Bud.Application.Features.Employees;

namespace Bud.Application.Features.Teams;

public static class TeamContractMapper
{
    public static TeamResponse ToResponse(this Team source)
    {
        var leaderEntry = source.EmployeeTeams.FirstOrDefault(et => et.Role == TeamRole.Leader);

        return new TeamResponse
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            Color = source.Color,
            Status = source.Status,
            OrganizationId = source.OrganizationId,
            ParentTeamId = source.ParentTeamId,
            LeaderId = source.LeaderId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt,
            DeletedAt = source.DeletedAt,
            ParentTeam = source.ParentTeam?.ToResponse(),
            Employees = source.EmployeeTeams
            .Where(et => et.Employee is not null)
            .Select(et => et.Employee!.ToEmployeeMembershipResponse())
            .ToList(),
            Leader = leaderEntry?.Employee?.ToEmployeeMembershipResponse()
        };
    }
}
