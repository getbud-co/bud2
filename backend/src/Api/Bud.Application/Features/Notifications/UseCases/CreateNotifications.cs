using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Notifications.UseCases;

public sealed record CreateNotificationsCommand(
    IReadOnlyList<Guid> RecipientEmployeeIds,
    Guid OrganizationId,
    string Title,
    string Message,
    string Category,
    Guid? ReferenceId = null,
    string? ReferenceType = null);

public sealed partial class CreateNotifications(
    INotificationRepository notificationRepository,
    ILogger<CreateNotifications> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result> ExecuteAsync(CreateNotificationsCommand command, CancellationToken cancellationToken = default)
    {
        LogCreatingNotifications(logger, command.Category, command.RecipientEmployeeIds.Count);

        if (command.RecipientEmployeeIds.Count == 0)
        {
            return Result.Success();
        }

        try
        {
            var notifications = command.RecipientEmployeeIds
                .Distinct()
                .Select(recipientId => Notification.Create(
                    Guid.NewGuid(),
                    recipientId,
                    command.OrganizationId,
                    command.Title,
                    command.Message,
                    command.Category,
                    DateTime.UtcNow,
                    command.ReferenceId,
                    command.ReferenceType))
                .ToList();

            await notificationRepository.AddRangeAsync(notifications, cancellationToken);
            await unitOfWork.CommitAsync(notificationRepository.SaveChangesAsync, cancellationToken);
            LogNotificationsCreated(logger, notifications.Count, command.Category);
            return Result.Success();
        }
        catch (DomainInvariantException ex)
        {
            LogNotificationsCreationFailed(logger, command.Category, ex.Message);
            return Result.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 4050, Level = LogLevel.Information, Message = "Creating notifications for category '{Category}' to {RequestedCount} recipients")]
    private static partial void LogCreatingNotifications(ILogger logger, string category, int requestedCount);

    [LoggerMessage(EventId = 4051, Level = LogLevel.Information, Message = "Notifications created successfully. Count: {Count}, Category: '{Category}'")]
    private static partial void LogNotificationsCreated(ILogger logger, int count, string category);

    [LoggerMessage(EventId = 4052, Level = LogLevel.Warning, Message = "Notifications creation failed for category '{Category}': {Reason}")]
    private static partial void LogNotificationsCreationFailed(ILogger logger, string category, string reason);
}
