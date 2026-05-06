using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Missions.UseCases;

public sealed record CreateMissionCommand(
    string Title,
    string? Description,
    string? Dimension,
    DateTime? DueDate,
    DateTime? CompletedAt,
    MissionStatus Status,
    MissionVisibility Visibility,
    Guid? CycleId,
    Guid? ParentId,
    Guid? EmployeeId);

public sealed partial class CreateMission(
    IMissionRepository missionRepository,
    IEmployeeRepository employeeRepository,
    ITenantProvider tenantProvider,
    ILogger<CreateMission> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<Mission>> ExecuteAsync(
        CreateMissionCommand command,
        CancellationToken cancellationToken = default)
    {
        LogCreatingMission(logger, command.Title);

        var organizationId = tenantProvider.TenantId;
        if (!organizationId.HasValue)
        {
            LogMissionCreationFailed(logger, command.Title, "Tenant not selected");
            return Result<Mission>.Forbidden(UserErrorMessages.MissionCreateForbidden);
        }

        if (command.ParentId.HasValue)
        {
            var parentMission = await missionRepository.GetByIdReadOnlyAsync(command.ParentId.Value, cancellationToken);
            if (parentMission is null)
            {
                LogMissionCreationFailed(logger, command.Title, UserErrorMessages.ParentMissionNotFound);
                return Result<Mission>.NotFound(UserErrorMessages.ParentMissionNotFound);
            }

            var violation = MissionDateRangePolicy.ValidateChildDueDate<Mission>(
                command.DueDate.HasValue ? UtcDateTimeNormalizer.Normalize(command.DueDate.Value) : null,
                parentMission.DueDate);
            if (violation is not null)
            {
                LogMissionCreationFailed(logger, command.Title, violation.Error!);
                return violation;
            }
        }

        try
        {
            var mission = Mission.Create(
                Guid.NewGuid(),
                organizationId.Value,
                command.Title,
                command.Description,
                command.Dimension,
                UtcDateTimeNormalizer.Normalize(command.DueDate.GetValueOrDefault()),
                UtcDateTimeNormalizer.Normalize(command.CompletedAt.GetValueOrDefault()),
                command.Status,
                command.Visibility,
                command.ParentId,
                tenantProvider.EmployeeId);

            mission.CycleId = command.CycleId;

            if (command.EmployeeId.HasValue)
            {
                var employee = await employeeRepository.GetByIdAsync(command.EmployeeId.Value, cancellationToken);
                if (employee is null)
                {
                    LogMissionCreationFailed(logger, command.Title, UserErrorMessages.EmployeeNotFound);
                    return Result<Mission>.NotFound(UserErrorMessages.EmployeeNotFound);
                }

                if (!employee.Memberships.Any(m => m.OrganizationId == organizationId.Value))
                {
                    LogMissionCreationFailed(logger, command.Title, "Employee belongs to different organization");
                    return Result<Mission>.Forbidden(UserErrorMessages.MissionCreateForbidden);
                }

                mission.EmployeeId = command.EmployeeId.Value;
            }

            await missionRepository.AddAsync(mission, cancellationToken);
            await unitOfWork.CommitAsync(missionRepository.SaveChangesAsync, cancellationToken);

            LogMissionCreated(logger, mission.Id, mission.Title);
            return Result<Mission>.Success(mission);
        }
        catch (DomainInvariantException ex)
        {
            LogMissionCreationFailed(logger, command.Title, ex.Message);
            return Result<Mission>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 4000, Level = LogLevel.Information, Message = "Creating mission '{Title}'")]
    private static partial void LogCreatingMission(ILogger logger, string title);

    [LoggerMessage(EventId = 4001, Level = LogLevel.Information, Message = "Mission created successfully: {MissionId} - '{Title}'")]
    private static partial void LogMissionCreated(ILogger logger, Guid missionId, string title);

    [LoggerMessage(EventId = 4002, Level = LogLevel.Warning, Message = "Mission creation failed for '{Title}': {Reason}")]
    private static partial void LogMissionCreationFailed(ILogger logger, string title, string reason);
}
