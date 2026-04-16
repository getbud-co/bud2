using Bud.Application.Common;
using Bud.Application.Features.Missions;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Tags.UseCases;

public sealed partial class AssignTagToMission(
    ITagRepository tagRepository,
    IMissionRepository missionRepository,
    IEmployeeRepository employeeRepository,
    ITenantProvider tenantProvider,
    ILogger<AssignTagToMission> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result> ExecuteAsync(
        Guid missionId,
        Guid tagId,
        CancellationToken cancellationToken = default)
    {
        LogAssigning(logger, missionId, tagId);

        if (!tenantProvider.EmployeeId.HasValue)
        {
            LogAssignFailed(logger, missionId, tagId, "Employee not identified");
            return Result.Forbidden(UserErrorMessages.TagAssignForbidden);
        }

        var mission = await missionRepository.GetByIdReadOnlyAsync(missionId, cancellationToken);
        if (mission is null)
        {
            LogAssignFailed(logger, missionId, tagId, "Mission not found");
            return Result.NotFound(UserErrorMessages.MissionNotFound);
        }

        var currentMember = await employeeRepository.GetByIdAsync(tenantProvider.EmployeeId.Value, cancellationToken);
        if (currentMember is null)
        {
            LogAssignFailed(logger, missionId, tagId, "Employee not found");
            return Result.Forbidden(UserErrorMessages.TagAssignForbidden);
        }

        var isContributor = currentMember.Memberships.Any(m =>
            m.OrganizationId == tenantProvider.TenantId!.Value && m.Role == EmployeeRole.Contributor);
        if (isContributor && mission.EmployeeId != tenantProvider.EmployeeId)
        {
            LogAssignFailed(logger, missionId, tagId, "Contributor can only tag their own missions");
            return Result.Forbidden(UserErrorMessages.TagAssignForbiddenContributor);
        }

        var tagExists = await tagRepository.ExistsAsync(tagId, cancellationToken);
        if (!tagExists)
        {
            LogAssignFailed(logger, missionId, tagId, "Tag not found");
            return Result.NotFound(UserErrorMessages.TagNotFound);
        }

        var existing = await tagRepository.GetMissionTagAsync(missionId, tagId, cancellationToken);
        if (existing is not null)
        {
            return Result.Success();
        }

        var missionTag = new MissionTag { MissionId = missionId, TagId = tagId };
        await tagRepository.AddMissionTagAsync(missionTag, cancellationToken);
        await unitOfWork.CommitAsync(tagRepository.SaveChangesAsync, cancellationToken);

        LogAssigned(logger, missionId, tagId);
        return Result.Success();
    }

    [LoggerMessage(EventId = 5012, Level = LogLevel.Information, Message = "Assigning tag {TagId} to mission {MissionId}")]
    private static partial void LogAssigning(ILogger logger, Guid missionId, Guid tagId);

    [LoggerMessage(EventId = 5013, Level = LogLevel.Information, Message = "Tag {TagId} assigned to mission {MissionId}")]
    private static partial void LogAssigned(ILogger logger, Guid missionId, Guid tagId);

    [LoggerMessage(EventId = 5014, Level = LogLevel.Warning, Message = "Tag assignment failed for mission {MissionId} / tag {TagId}: {Reason}")]
    private static partial void LogAssignFailed(ILogger logger, Guid missionId, Guid tagId, string reason);
}
