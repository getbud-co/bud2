using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;

namespace Bud.Application.Features.Notifications.UseCases;

public sealed class PatchNotification(
    INotificationRepository notificationRepository,
    ITenantProvider tenantProvider,
    IApplicationAuthorizationGateway authorizationGateway,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result> ExecuteAsync(
        ClaimsPrincipal user,
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        if (tenantProvider.EmployeeId is null)
        {
            return Result.Forbidden(UserErrorMessages.EmployeeNotIdentified);
        }

        var notification = await notificationRepository.GetByIdAsync(notificationId, cancellationToken);
        if (notification is null)
        {
            return Result.NotFound(UserErrorMessages.NotificationNotFound);
        }

        var canWrite = await authorizationGateway.CanWriteAsync(user, new NotificationResource(notificationId), cancellationToken);
        if (!canWrite)
        {
            return Result.Forbidden(UserErrorMessages.NotificationPatchForbidden);
        }

        if (notification.IsRead)
        {
            return Result.Success();
        }

        notification.MarkAsRead(DateTime.UtcNow);
        await unitOfWork.CommitAsync(notificationRepository.SaveChangesAsync, cancellationToken);

        return Result.Success();
    }
}
