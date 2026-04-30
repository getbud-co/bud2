using Bud.Application.Common;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Teams.UseCases;

public sealed record PatchTeamCommand(
    Optional<string> Name,
    Optional<string?> Description,
    Optional<TeamColor> Color,
    Optional<TeamStatus> Status,
    Optional<Guid> LeaderId,
    Optional<Guid?> ParentTeamId);

public sealed partial class PatchTeam(
    ITeamRepository teamRepository,
    IEmployeeRepository employeeRepository,
    ILogger<PatchTeam> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<Team>> ExecuteAsync(
        Guid id,
        PatchTeamCommand command,
        CancellationToken cancellationToken = default)
    {
        LogPatchingTeam(logger, id);

        var team = await teamRepository.GetByIdWithEmployeeTeamsAsync(id, cancellationToken);
        if (team is null)
        {
            LogTeamPatchFailed(logger, id, "Not found");
            return Result<Team>.NotFound(UserErrorMessages.TeamNotFound);
        }

        var requestedParentTeamId = command.ParentTeamId.HasValue ? command.ParentTeamId.Value : team.ParentTeamId;
        var requestedName = command.Name.HasValue ? (command.Name.Value ?? team.Name) : team.Name;

        if (command.ParentTeamId.HasValue && requestedParentTeamId != team.ParentTeamId)
        {
            if (requestedParentTeamId.HasValue && requestedParentTeamId.Value == id)
            {
                LogTeamPatchFailed(logger, id, "Team cannot be its own parent");
                return Result<Team>.Failure(UserErrorMessages.TeamSelfParentForbidden);
            }

            if (requestedParentTeamId.HasValue)
            {
                var parentTeam = await teamRepository.GetByIdAsync(requestedParentTeamId.Value, cancellationToken);
                if (parentTeam is null)
                {
                    LogTeamPatchFailed(logger, id, "Parent team not found");
                    return Result<Team>.NotFound(UserErrorMessages.ParentTeamNotFound);
                }

                if (parentTeam.OrganizationId != team.OrganizationId)
                {
                    LogTeamPatchFailed(logger, id, "Parent team belongs to different organization");
                    return Result<Team>.Failure(UserErrorMessages.TeamParentMustBeSameOrganization);
                }
            }
        }

        if (command.LeaderId.HasValue)
        {
            var leaderValidation = await EmployeeLeadershipPolicy.ValidateLeaderForOrganizationAsync<Team>(
                employeeRepository,
                command.LeaderId.Value,
                team.OrganizationId,
                cancellationToken);
            if (leaderValidation is not null)
            {
                LogTeamPatchFailed(logger, id, "Leader validation failed");
                return leaderValidation;
            }
        }

        try
        {
            team.Rename(requestedName);
            team.Reparent(requestedParentTeamId, team.Id);

            if (command.Description.HasValue)
            {
                team.Describe(command.Description.Value);
            }

            if (command.Color.HasValue)
            {
                team.SetColor(command.Color.Value);
            }

            if (command.Status.HasValue)
            {
                team.SetStatus(command.Status.Value);
            }

            if (command.LeaderId.HasValue)
            {
                team.AssignLeader(command.LeaderId.Value);
            }

            await unitOfWork.CommitAsync(teamRepository.SaveChangesAsync, cancellationToken);

            LogTeamPatched(logger, id, team.Name);
            return Result<Team>.Success(team);
        }
        catch (DomainInvariantException ex)
        {
            LogTeamPatchFailed(logger, id, ex.Message);
            return Result<Team>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 4033, Level = LogLevel.Information, Message = "Patching team {TeamId}")]
    private static partial void LogPatchingTeam(ILogger logger, Guid teamId);

    [LoggerMessage(EventId = 4034, Level = LogLevel.Information, Message = "Team patched successfully: {TeamId} - '{Name}'")]
    private static partial void LogTeamPatched(ILogger logger, Guid teamId, string name);

    [LoggerMessage(EventId = 4035, Level = LogLevel.Warning, Message = "Team patch failed for {TeamId}: {Reason}")]
    private static partial void LogTeamPatchFailed(ILogger logger, Guid teamId, string reason);
}
