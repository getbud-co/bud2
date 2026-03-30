using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Missions.UseCases;

public sealed record CreateMissionCommand(
    string Name,
    string? Description,
    string? Dimension,
    DateTime StartDate,
    DateTime EndDate,
    MissionStatus Status,
    Guid? ParentId,
    Guid? EmployeeId);

public sealed partial class CreateMission(
    IMissionRepository missionRepository,
    IEmployeeRepository employeeRepository,
    ITenantProvider tenantProvider,
    ILogger<CreateMission> logger,
    IUnitOfWork? unitOfWork = null,
    IApplicationAuthorizationGateway? authorizationGateway = null)
{
    public Task<Result<Mission>> ExecuteAsync(
        ClaimsPrincipal user,
        CreateMissionCommand command,
        CancellationToken cancellationToken = default)
        => ExecuteAsyncInternal(user, command, cancellationToken);

    public async Task<Result<Mission>> ExecuteAsync(
        CreateMissionCommand command,
        CancellationToken cancellationToken = default)
        => await ExecuteAsyncInternal(new ClaimsPrincipal(new ClaimsIdentity()), command, cancellationToken);

    private async Task<Result<Mission>> ExecuteAsyncInternal(
        ClaimsPrincipal user,
        CreateMissionCommand command,
        CancellationToken cancellationToken)
    {
        LogCreatingMission(logger, command.Name);

        var organizationId = tenantProvider.TenantId;
        if (!organizationId.HasValue)
        {
            LogMissionCreationFailed(logger, command.Name, "Tenant not selected");
            return Result<Mission>.Forbidden(UserErrorMessages.MissionCreateForbidden);
        }

        if (authorizationGateway is not null)
        {
            var canWrite = await authorizationGateway.CanWriteAsync(
                user,
                new CreateMissionContext(organizationId.Value),
                cancellationToken);
            if (!canWrite)
            {
                LogMissionCreationFailed(logger, command.Name, UserErrorMessages.MissionCreateForbidden);
                return Result<Mission>.Forbidden(UserErrorMessages.MissionCreateForbidden);
            }
        }

        Mission? parentMission = null;
        if (command.ParentId.HasValue)
        {
            parentMission = await missionRepository.GetByIdReadOnlyAsync(command.ParentId.Value, cancellationToken);
            if (parentMission is null)
            {
                LogMissionCreationFailed(logger, command.Name, UserErrorMessages.ParentMissionNotFound);
                return Result<Mission>.NotFound(UserErrorMessages.ParentMissionNotFound);
            }
        }

        if (parentMission is not null)
        {
            var violation = MissionDateRangePolicy.ValidateChildStartDate<Mission>(
                UtcDateTimeNormalizer.Normalize(command.StartDate), parentMission.StartDate);
            if (violation is not null)
            {
                LogMissionCreationFailed(logger, command.Name, violation.Error!);
                return violation;
            }
        }

        try
        {
            var mission = Mission.Create(
                Guid.NewGuid(),
                organizationId.Value,
                command.Name,
                command.Description,
                command.Dimension,
                UtcDateTimeNormalizer.Normalize(command.StartDate),
                UtcDateTimeNormalizer.Normalize(command.EndDate),
                command.Status,
                parentMission?.Id,
                tenantProvider.EmployeeId);

            if (command.EmployeeId.HasValue)
            {
                var employee = await employeeRepository.GetByIdAsync(command.EmployeeId.Value, cancellationToken);
                if (employee is null)
                {
                    LogMissionCreationFailed(logger, command.Name, UserErrorMessages.EmployeeNotFound);
                    return Result<Mission>.NotFound(UserErrorMessages.EmployeeNotFound);
                }

                if (employee.OrganizationId != organizationId.Value)
                {
                    LogMissionCreationFailed(logger, command.Name, "Employee belongs to different organization");
                    return Result<Mission>.Forbidden(UserErrorMessages.MissionCreateForbidden);
                }

                mission.EmployeeId = command.EmployeeId.Value;
            }

            await missionRepository.AddAsync(mission, cancellationToken);
            await unitOfWork.CommitAsync(missionRepository.SaveChangesAsync, cancellationToken);

            LogMissionCreated(logger, mission.Id, mission.Name);
            return Result<Mission>.Success(mission);
        }
        catch (DomainInvariantException ex)
        {
            LogMissionCreationFailed(logger, command.Name, ex.Message);
            return Result<Mission>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 4000, Level = LogLevel.Information, Message = "Creating mission '{Name}'")]
    private static partial void LogCreatingMission(ILogger logger, string name);

    [LoggerMessage(EventId = 4001, Level = LogLevel.Information, Message = "Mission created successfully: {MissionId} - '{Name}'")]
    private static partial void LogMissionCreated(ILogger logger, Guid missionId, string name);

    [LoggerMessage(EventId = 4002, Level = LogLevel.Warning, Message = "Mission creation failed for '{Name}': {Reason}")]
    private static partial void LogMissionCreationFailed(ILogger logger, string name, string reason);
}
