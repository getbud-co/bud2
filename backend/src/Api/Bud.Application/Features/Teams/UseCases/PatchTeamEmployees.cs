using Bud.Application.Common;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Teams.UseCases;

public sealed record PatchTeamEmployeesCommand(List<Guid> EmployeeIds);

public sealed partial class PatchTeamEmployees(
    ITeamRepository teamRepository,
    IEmployeeRepository employeeRepository,
    ILogger<PatchTeamEmployees> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result> ExecuteAsync(
        Guid id,
        PatchTeamEmployeesCommand command,
        CancellationToken cancellationToken = default)
    {
        LogPatchingTeamEmployees(logger, id);

        var team = await teamRepository.GetByIdWithEmployeeTeamsAsync(id, cancellationToken);
        if (team is null)
        {
            LogTeamEmployeesPatchFailed(logger, id, "Team not found");
            return Result.NotFound(UserErrorMessages.TeamNotFound);
        }

        var distinctEmployeeIds = command.EmployeeIds.Distinct().ToList();

        var currentLeaderId = team.LeaderId;
        if (currentLeaderId.HasValue && !distinctEmployeeIds.Contains(currentLeaderId.Value))
        {
            LogTeamEmployeesPatchFailed(logger, id, "Leader not in members list");
            return Result.Failure(UserErrorMessages.TeamLeaderMustBeMember, ErrorType.Validation);
        }

        if (distinctEmployeeIds.Count > 0)
        {
            var validCount = await employeeRepository.CountByIdsAndOrganizationAsync(
                distinctEmployeeIds,
                team.OrganizationId,
                cancellationToken);

            if (validCount != distinctEmployeeIds.Count)
            {
                LogTeamEmployeesPatchFailed(logger, id, "Invalid employees");
                return Result.Failure(UserErrorMessages.TeamMembersInvalid, ErrorType.Validation);
            }
        }

        team.EmployeeTeams.Clear();

        foreach (var employeeId in distinctEmployeeIds)
        {
            var role = employeeId == currentLeaderId ? TeamRole.Leader : TeamRole.Member;
            team.EmployeeTeams.Add(new EmployeeTeam
            {
                EmployeeId = employeeId,
                TeamId = id,
                Role = role,
                AssignedAt = DateTime.UtcNow
            });
        }

        await unitOfWork.CommitAsync(teamRepository.SaveChangesAsync, cancellationToken);
        LogTeamEmployeesPatched(logger, id, distinctEmployeeIds.Count);
        return Result.Success();
    }

    [LoggerMessage(EventId = 4039, Level = LogLevel.Information, Message = "Patching employees for team {TeamId}")]
    private static partial void LogPatchingTeamEmployees(ILogger logger, Guid teamId);

    [LoggerMessage(EventId = 4040, Level = LogLevel.Information, Message = "Team employees patched successfully: {TeamId} with {Count} members")]
    private static partial void LogTeamEmployeesPatched(ILogger logger, Guid teamId, int count);

    [LoggerMessage(EventId = 4041, Level = LogLevel.Warning, Message = "Team employees patch failed for {TeamId}: {Reason}")]
    private static partial void LogTeamEmployeesPatchFailed(ILogger logger, Guid teamId, string reason);
}
