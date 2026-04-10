using Bud.Application.Common;
using Bud.Application.Ports;

namespace Bud.Application.Features.Notifications.UseCases;

public sealed class PatchNotification(
    INotificationRepository notificationRepository,
    ITenantProvider tenantProvider,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result> ExecuteAsync(
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

        if (notification.RecipientEmployeeId != tenantProvider.EmployeeId)
        {
            return Result.NotFound(UserErrorMessages.NotificationNotFound);
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
