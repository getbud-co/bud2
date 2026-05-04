using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Notifications.UseCases;

public sealed partial class UpdateAllNotifications(
    INotificationRepository notificationRepository,
    ITenantProvider tenantProvider,
    ILogger<UpdateAllNotifications> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        LogUpdatingNotifications(logger);

        if (!tenantProvider.EmployeeId.HasValue)
        {
            LogNotificationsUpdateFailed(logger, "Employee context not found");
            return Result.Forbidden("Funcionário não identificado.");
        }

        var unreadNotifications = await notificationRepository.GetUnreadByRecipientAsync(
            tenantProvider.EmployeeId.Value,
            cancellationToken);

        var readAtUtc = DateTime.UtcNow;
        foreach (var notification in unreadNotifications)
        {
            notification.MarkAsRead(readAtUtc);
        }

        await unitOfWork.CommitAsync(notificationRepository.SaveChangesAsync, cancellationToken);
        LogNotificationsUpdated(logger, unreadNotifications.Count);
        return Result.Success();
    }

    [LoggerMessage(EventId = 4056, Level = LogLevel.Information, Message = "Updating notifications collection")]
    private static partial void LogUpdatingNotifications(ILogger logger);

    [LoggerMessage(EventId = 4057, Level = LogLevel.Information, Message = "Notifications collection updated successfully. Count: {Count}")]
    private static partial void LogNotificationsUpdated(ILogger logger, int count);

    [LoggerMessage(EventId = 4058, Level = LogLevel.Warning, Message = "Notifications collection update failed: {Reason}")]
    private static partial void LogNotificationsUpdateFailed(ILogger logger, string reason);
}
