using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Missions.UseCases;

public sealed record PatchMissionCommand(
    Optional<string> Title,
    Optional<string?> Description,
    Optional<string?> Dimension,
    Optional<DateTime?> DueDate,
    Optional<DateTime?> CompletedAt,
    Optional<MissionStatus> Status,
    Optional<MissionVisibility> Visibility,
    Optional<Guid?> CycleId,
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

        if (mission.ParentId.HasValue && command.DueDate.HasValue)
        {
            var parentMission = await missionRepository.GetByIdReadOnlyAsync(mission.ParentId.Value, cancellationToken);
            if (parentMission is not null)
            {
                var childDueDate = command.DueDate.Value.HasValue
                    ? UtcDateTimeNormalizer.Normalize(command.DueDate.Value.Value)
                    : mission.DueDate;

                var violation = MissionDateRangePolicy.ValidateChildDueDate<Mission>(childDueDate, parentMission.DueDate);
                if (violation is not null)
                {
                    LogMissionPatchFailed(logger, id, violation.Error!);
                    return violation;
                }
            }
        }

        try
        {
            var title = command.Title.HasValue ? (command.Title.Value ?? mission.Title) : mission.Title;
            var description = command.Description.HasValue ? command.Description.Value : mission.Description;
            var dimension = command.Dimension.HasValue ? command.Dimension.Value : mission.Dimension;
            var dueDate = command.DueDate.HasValue ? command.DueDate.Value : mission.DueDate;
            var completedAt = command.CompletedAt.HasValue ? command.CompletedAt.Value : mission.CompletedAt;
            var status = command.Status.HasValue ? command.Status.Value : mission.Status;
            var visibility = command.Visibility.HasValue ? command.Visibility.Value : mission.Visibility;

            if (command.CycleId.HasValue)
            {
                mission.CycleId = command.CycleId.Value;
            }

            mission.UpdateDetails(
                title,
                description,
                dimension,
                dueDate.HasValue ? UtcDateTimeNormalizer.Normalize(dueDate.Value) : default,
                completedAt.HasValue ? UtcDateTimeNormalizer.Normalize(completedAt.Value) : default,
                status,
                visibility);

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

            LogMissionPatched(logger, id, mission.Title);
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

    [LoggerMessage(EventId = 4004, Level = LogLevel.Information, Message = "Mission patched successfully: {MissionId} - '{Title}'")]
    private static partial void LogMissionPatched(ILogger logger, Guid missionId, string title);

    [LoggerMessage(EventId = 4005, Level = LogLevel.Warning, Message = "Mission patch failed for {MissionId}: {Reason}")]
    private static partial void LogMissionPatchFailed(ILogger logger, Guid missionId, string reason);
}
