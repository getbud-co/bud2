using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Notifications.UseCases;

public sealed partial class UpdateNotification(
    INotificationRepository notificationRepository,
    ITenantProvider tenantProvider,
    ILogger<UpdateNotification> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<Notification>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        LogUpdatingNotification(logger, id);

        if (!tenantProvider.EmployeeId.HasValue)
        {
            LogNotificationUpdateFailed(logger, id, "Employee context not found");
            return Result<Notification>.Forbidden("Funcionário não identificado.");
        }

        var notification = await notificationRepository.GetByIdAsync(id, cancellationToken);
        if (notification is null || notification.RecipientEmployeeId != tenantProvider.EmployeeId.Value)
        {
            LogNotificationUpdateFailed(logger, id, "Not found");
            return Result<Notification>.NotFound("Notificação não encontrada.");
        }

        notification.MarkAsRead(DateTime.UtcNow);
        await unitOfWork.CommitAsync(notificationRepository.SaveChangesAsync, cancellationToken);
        LogNotificationUpdated(logger, id);
        return Result<Notification>.Success(notification);
    }

    [LoggerMessage(EventId = 4053, Level = LogLevel.Information, Message = "Updating notification {NotificationId}")]
    private static partial void LogUpdatingNotification(ILogger logger, Guid notificationId);

    [LoggerMessage(EventId = 4054, Level = LogLevel.Information, Message = "Notification updated successfully: {NotificationId}")]
    private static partial void LogNotificationUpdated(ILogger logger, Guid notificationId);

    [LoggerMessage(EventId = 4055, Level = LogLevel.Warning, Message = "Notification update failed for {NotificationId}: {Reason}")]
    private static partial void LogNotificationUpdateFailed(ILogger logger, Guid notificationId, string reason);
}
