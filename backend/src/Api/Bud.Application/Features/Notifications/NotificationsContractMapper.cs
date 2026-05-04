namespace Bud.Application.Features.Notifications;

public static class NotificationsContractMapper
{
    public static NotificationResponse ToResponse(this Notification source)
    {
        return new NotificationResponse
        {
            Id = source.Id,
            Title = source.Title,
            Message = source.Message,
            Category = source.Category,
            IsRead = source.IsRead,
            CreatedAtUtc = source.CreatedAtUtc,
            ReadAtUtc = source.ReadAtUtc,
            ReferenceId = source.ReferenceId,
            ReferenceType = source.ReferenceType
        };
    }
}
