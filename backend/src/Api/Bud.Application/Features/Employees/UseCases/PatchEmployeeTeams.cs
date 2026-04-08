using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Employees.UseCases;

public sealed record PatchEmployeeTeamsCommand(List<Guid> TeamIds);

public sealed partial class PatchEmployeeTeams(
    IEmployeeRepository employeeRepository,
    IApplicationAuthorizationGateway authorizationGateway,
    ILogger<PatchEmployeeTeams> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result> ExecuteAsync(
        ClaimsPrincipal user,
        Guid id,
        PatchEmployeeTeamsCommand command,
        CancellationToken cancellationToken = default)
    {
        LogPatchingEmployeeTeams(logger, id);

        var member = await employeeRepository.GetByIdWithEmployeeTeamsAsync(id, cancellationToken);
        if (member is null)
        {
            LogEmployeeTeamsPatchFailed(logger, id, "Employee not found");
            return Result.NotFound(UserErrorMessages.EmployeeNotFound);
        }

        var canAssign = await authorizationGateway.CanWriteAsync(user, new EmployeeResource(id), cancellationToken);
        if (!canAssign)
        {
            LogEmployeeTeamsPatchFailed(logger, id, "Forbidden");
            return Result.Forbidden(UserErrorMessages.EmployeeAssignTeamsForbidden);
        }

        var distinctTeamIds = command.TeamIds.Distinct().ToList();

        if (distinctTeamIds.Count > 0)
        {
            var validCount = await employeeRepository.CountTeamsByIdsAndOrganizationAsync(
                distinctTeamIds,
                member.OrganizationId,
                cancellationToken);

            if (validCount != distinctTeamIds.Count)
            {
                LogEmployeeTeamsPatchFailed(logger, id, "Invalid teams");
                return Result.Failure(UserErrorMessages.EmployeeTeamsInvalid, ErrorType.Validation);
            }
        }

        member.Employee.EmployeeTeams.Clear();

        foreach (var teamId in distinctTeamIds)
        {
            member.Employee.EmployeeTeams.Add(new EmployeeTeam
            {
                EmployeeId = id,
                TeamId = teamId,
                AssignedAt = DateTime.UtcNow,
            });
        }

        await unitOfWork.CommitAsync(employeeRepository.SaveChangesAsync, cancellationToken);
        LogEmployeeTeamsPatched(logger, id, distinctTeamIds.Count);
        return Result.Success();
    }

    [LoggerMessage(EventId = 4049, Level = LogLevel.Information, Message = "Patching teams for employee {EmployeeId}")]
    private static partial void LogPatchingEmployeeTeams(ILogger logger, Guid employeeId);

    [LoggerMessage(EventId = 4050, Level = LogLevel.Information, Message = "Employee teams patched successfully: {EmployeeId} with {Count} teams")]
    private static partial void LogEmployeeTeamsPatched(ILogger logger, Guid employeeId, int count);

    [LoggerMessage(EventId = 4051, Level = LogLevel.Warning, Message = "Employee teams patch failed for {EmployeeId}: {Reason}")]
    private static partial void LogEmployeeTeamsPatchFailed(ILogger logger, Guid employeeId, string reason);
}
