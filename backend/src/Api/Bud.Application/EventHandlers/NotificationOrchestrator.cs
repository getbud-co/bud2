using Bud.Application.Common;

namespace Bud.Application.EventHandlers;

/// <summary>
/// Coordinates notification creation for domain events.
/// </summary>
public class NotificationOrchestrator
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationRecipientResolver _notificationRecipientResolver;
    private readonly IUnitOfWork? _unitOfWork;

    public NotificationOrchestrator(
        INotificationRepository notificationRepository,
        INotificationRecipientResolver notificationRecipientResolver)
        : this(notificationRepository, notificationRecipientResolver, null)
    {
    }

    public NotificationOrchestrator(
        INotificationRepository notificationRepository,
        INotificationRecipientResolver notificationRecipientResolver,
        IUnitOfWork? unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _notificationRecipientResolver = notificationRecipientResolver;
        _unitOfWork = unitOfWork;
    }

    public virtual async Task NotifyMissionCreatedAsync(
        Guid missionId,
        Guid organizationId,
        string missionName,
        Guid? actorEmployeeId,
        CancellationToken cancellationToken = default)
    {
        var actorName = actorEmployeeId.HasValue
            ? await _notificationRecipientResolver.ResolveEmployeeNameAsync(actorEmployeeId.Value, cancellationToken)
            : null;

        var message = actorName is not null
            ? $"{actorName} criou a meta '{missionName}'"
            : $"Uma nova meta foi criada: '{missionName}'";

        await NotifyMissionEventAsync(missionId, organizationId, "Nova meta criada", message, NotificationType.MissionCreated, cancellationToken);
    }

    public virtual async Task NotifyMissionUpdatedAsync(
        Guid missionId,
        Guid organizationId,
        string missionName,
        Guid? actorEmployeeId,
        CancellationToken cancellationToken = default)
    {
        var actorName = actorEmployeeId.HasValue
            ? await _notificationRecipientResolver.ResolveEmployeeNameAsync(actorEmployeeId.Value, cancellationToken)
            : null;

        var message = actorName is not null
            ? $"{actorName} atualizou a meta '{missionName}'"
            : $"A meta '{missionName}' foi atualizada";

        await NotifyMissionEventAsync(missionId, organizationId, "Meta atualizada", message, NotificationType.MissionUpdated, cancellationToken);
    }

    public virtual async Task NotifyMissionDeletedAsync(
        Guid missionId,
        Guid organizationId,
        string missionName,
        Guid? actorEmployeeId,
        CancellationToken cancellationToken = default)
    {
        var actorName = actorEmployeeId.HasValue
            ? await _notificationRecipientResolver.ResolveEmployeeNameAsync(actorEmployeeId.Value, cancellationToken)
            : null;

        var message = actorName is not null
            ? $"{actorName} removeu a meta '{missionName}'"
            : $"A meta '{missionName}' foi removida";

        await NotifyMissionEventAsync(missionId, organizationId, "Meta removida", message, NotificationType.MissionDeleted, cancellationToken);
    }

    public virtual async Task NotifyCheckinCreatedAsync(
        Guid checkinId,
        Guid indicatorId,
        Guid organizationId,
        Guid? excludeEmployeeId,
        string indicatorName,
        CancellationToken cancellationToken = default)
    {
        var missionId = await _notificationRecipientResolver.ResolveMissionIdFromIndicatorAsync(indicatorId, cancellationToken);
        if (!missionId.HasValue)
        {
            return;
        }

        var recipients = await _notificationRecipientResolver.ResolveMissionRecipientsAsync(
            missionId.Value,
            organizationId,
            excludeEmployeeId,
            cancellationToken);

        if (recipients.Count == 0)
        {
            return;
        }

        var actorName = excludeEmployeeId.HasValue
            ? await _notificationRecipientResolver.ResolveEmployeeNameAsync(excludeEmployeeId.Value, cancellationToken)
            : null;

        var message = actorName is not null
            ? $"{actorName} registrou um check-in no indicador '{indicatorName}'"
            : $"Um novo check-in foi registrado no indicador '{indicatorName}'";

        await CreateNotificationsAsync(
            recipients,
            organizationId,
            "Novo check-in registrado",
            message,
            NotificationType.CheckinCreated,
            checkinId,
            "Checkin",
            cancellationToken);
    }

    private async Task NotifyMissionEventAsync(
        Guid missionId,
        Guid organizationId,
        string title,
        string message,
        NotificationType type,
        CancellationToken cancellationToken)
    {
        var recipients = await _notificationRecipientResolver.ResolveMissionRecipientsAsync(
            missionId,
            organizationId,
            null,
            cancellationToken);

        if (recipients.Count == 0)
        {
            return;
        }

        await CreateNotificationsAsync(
            recipients,
            organizationId,
            title,
            message,
            type,
            missionId,
            "Mission",
            cancellationToken);
    }

    private async Task CreateNotificationsAsync(
        IEnumerable<Guid> recipientIds,
        Guid organizationId,
        string title,
        string message,
        NotificationType type,
        Guid? relatedEntityId,
        string? relatedEntityType,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var notifications = recipientIds.Select(recipientId => Notification.Create(
            Guid.NewGuid(),
            recipientId,
            organizationId,
            title,
            message,
            type,
            now,
            relatedEntityId,
            relatedEntityType)).ToList();

        if (notifications.Count == 0)
        {
            return;
        }

        await _notificationRepository.AddRangeAsync(notifications, cancellationToken);
        await _unitOfWork.CommitAsync(_notificationRepository.SaveChangesAsync, cancellationToken);
    }
}
