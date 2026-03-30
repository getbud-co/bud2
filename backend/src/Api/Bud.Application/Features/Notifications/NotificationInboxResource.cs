namespace Bud.Application.Features.Notifications;

public sealed class NotificationInboxResource
{
    public static NotificationInboxResource Instance { get; } = new();

    private NotificationInboxResource()
    {
    }
}
