using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Teams.UseCases;

public sealed record CreateTeamCommand(string Name, Guid OrganizationId, Guid LeaderId, Guid? ParentTeamId);

public sealed partial class CreateTeam(
    ITeamRepository teamRepository,
    IEmployeeRepository employeeRepository,
    ITenantProvider tenantProvider,
    IApplicationAuthorizationGateway authorizationGateway,
    ILogger<CreateTeam> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<Team>> ExecuteAsync(
        ClaimsPrincipal user,
        CreateTeamCommand command,
        CancellationToken cancellationToken = default)
    {
        LogCreatingTeam(logger, command.Name);

        if (tenantProvider.TenantId is null)
        {
            LogTeamCreationFailed(logger, command.Name, "Tenant not selected");
            return Result<Team>.Forbidden(UserErrorMessages.TeamCreateForbidden);
        }

        var organizationId = tenantProvider.TenantId.Value;

        var canCreate = await authorizationGateway.CanWriteAsync(
            user,
            new CreateTeamContext(organizationId),
            cancellationToken);
        if (!canCreate)
        {
            LogTeamCreationFailed(logger, command.Name, "Forbidden");
            return Result<Team>.Forbidden(UserErrorMessages.TeamCreateForbidden);
        }

        if (command.ParentTeamId.HasValue)
        {
            var parentTeam = await teamRepository.GetByIdAsync(command.ParentTeamId.Value, cancellationToken);
            if (parentTeam is null)
            {
                LogTeamCreationFailed(logger, command.Name, "Parent team not found");
                return Result<Team>.NotFound(UserErrorMessages.ParentTeamNotFound);
            }

            if (parentTeam.OrganizationId != organizationId)
            {
                LogTeamCreationFailed(logger, command.Name, "Parent team belongs to different organization");
                return Result<Team>.Failure(UserErrorMessages.TeamParentMustBeSameOrganization);
            }
        }

        var leaderValidation = await EmployeeLeadershipPolicy.ValidateLeaderForOrganizationAsync<Team>(
            employeeRepository,
            command.LeaderId,
            organizationId,
            cancellationToken);
        if (leaderValidation is not null)
        {
            LogTeamCreationFailed(logger, command.Name, "Leader validation failed");
            return leaderValidation;
        }

        try
        {
            var team = Team.Create(
                Guid.NewGuid(),
                organizationId,
                command.Name,
                command.LeaderId,
                command.ParentTeamId);

            team.EmployeeTeams.Add(new EmployeeTeam
            {
                EmployeeId = command.LeaderId,
                TeamId = team.Id,
                AssignedAt = DateTime.UtcNow
            });

            await teamRepository.AddAsync(team, cancellationToken);
            await unitOfWork.CommitAsync(teamRepository.SaveChangesAsync, cancellationToken);

            LogTeamCreated(logger, team.Id, team.Name);
            return Result<Team>.Success(team);
        }
        catch (DomainInvariantException ex)
        {
            LogTeamCreationFailed(logger, command.Name, ex.Message);
            return Result<Team>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 4030, Level = LogLevel.Information, Message = "Creating team '{Name}'")]
    private static partial void LogCreatingTeam(ILogger logger, string name);

    [LoggerMessage(EventId = 4031, Level = LogLevel.Information, Message = "Team created successfully: {TeamId} - '{Name}'")]
    private static partial void LogTeamCreated(ILogger logger, Guid teamId, string name);

    [LoggerMessage(EventId = 4032, Level = LogLevel.Warning, Message = "Team creation failed for '{Name}': {Reason}")]
    private static partial void LogTeamCreationFailed(ILogger logger, string name, string reason);
}
