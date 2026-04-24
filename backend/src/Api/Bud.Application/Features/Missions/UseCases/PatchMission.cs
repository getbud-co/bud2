using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Missions.UseCases;

public sealed record PatchMissionCommand(
    Optional<string> Name,
    Optional<string?> Description,
    Optional<string?> Dimension,
    Optional<DateTime> StartDate,
    Optional<DateTime> EndDate,
    Optional<MissionStatus> Status,
    Optional<Guid?> EmployeeId);

public sealed partial class PatchMission(
    IMissionRepository missionRepository,
    IEmployeeRepository employeeRepository,
    ITenantProvider tenantProvider,
    ILogger<PatchMission> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<Mission>> ExecuteAsync(
        Guid id,
        PatchMissionCommand command,
        CancellationToken cancellationToken = default)
    {
        LogPatchingMission(logger, id);

        var mission = await missionRepository.GetByIdAsync(id, cancellationToken);
        if (mission is null)
        {
            LogMissionPatchFailed(logger, id, "Not found");
            return Result<Mission>.NotFound(UserErrorMessages.MissionNotFound);
        }

        if (mission.ParentId.HasValue && (command.StartDate.HasValue || command.EndDate.HasValue))
        {
            var parentMission = await missionRepository.GetByIdReadOnlyAsync(mission.ParentId.Value, cancellationToken);
            if (parentMission is not null)
            {
                var childStartDate = command.StartDate.HasValue
                    ? UtcDateTimeNormalizer.Normalize(command.StartDate.Value)
                    : mission.StartDate;
                var childEndDate = command.EndDate.HasValue
                    ? UtcDateTimeNormalizer.Normalize(command.EndDate.Value)
                    : mission.EndDate;

                var violation = MissionDateRangePolicy.ValidateChildWindow<Mission>(
                    childStartDate,
                    childEndDate,
                    parentMission.StartDate,
                    parentMission.EndDate);
                if (violation is not null)
                {
                    LogMissionPatchFailed(logger, id, violation.Error!);
                    return violation;
                }
            }
        }

        try
        {
            var status = command.Status.HasValue ? command.Status.Value : mission.Status;
            var name = command.Name.HasValue ? (command.Name.Value ?? mission.Name) : mission.Name;
            var description = command.Description.HasValue ? command.Description.Value : mission.Description;
            var dimension = command.Dimension.HasValue ? command.Dimension.Value : mission.Dimension;
            var startDate = command.StartDate.HasValue ? command.StartDate.Value : mission.StartDate;
            var endDate = command.EndDate.HasValue ? command.EndDate.Value : mission.EndDate;

            mission.UpdateDetails(
                name,
                description,
                dimension,
                UtcDateTimeNormalizer.Normalize(startDate),
                UtcDateTimeNormalizer.Normalize(endDate),
                status);

            if (command.EmployeeId.HasValue)
            {
                var newEmployeeId = command.EmployeeId.Value;
                if (newEmployeeId.HasValue)
                {
                    var employee = await employeeRepository.GetByIdAsync(newEmployeeId.Value, cancellationToken);
                    if (employee is null)
                    {
                        LogMissionPatchFailed(logger, id, UserErrorMessages.EmployeeNotFound);
                        return Result<Mission>.NotFound(UserErrorMessages.EmployeeNotFound);
                    }

                    if (!employee.Memberships.Any(m => m.OrganizationId == mission.OrganizationId))
                    {
                        LogMissionPatchFailed(logger, id, "Employee belongs to different organization");
                        return Result<Mission>.Forbidden(UserErrorMessages.MissionUpdateForbidden);
                    }
                }

                mission.EmployeeId = newEmployeeId;
            }

            mission.MarkAsUpdated(tenantProvider.EmployeeId);
            await unitOfWork.CommitAsync(missionRepository.SaveChangesAsync, cancellationToken);

            LogMissionPatched(logger, id, mission.Name);
            return Result<Mission>.Success(mission);
        }
        catch (DomainInvariantException ex)
        {
            LogMissionPatchFailed(logger, id, ex.Message);
            return Result<Mission>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 4003, Level = LogLevel.Information, Message = "Patching mission {MissionId}")]
    private static partial void LogPatchingMission(ILogger logger, Guid missionId);

    [LoggerMessage(EventId = 4004, Level = LogLevel.Information, Message = "Mission patched successfully: {MissionId} - '{Name}'")]
    private static partial void LogMissionPatched(ILogger logger, Guid missionId, string name);

    [LoggerMessage(EventId = 4005, Level = LogLevel.Warning, Message = "Mission patch failed for {MissionId}: {Reason}")]
    private static partial void LogMissionPatchFailed(ILogger logger, Guid missionId, string reason);
}
