
namespace Bud.Application.EventHandlers;

public sealed class MissionCreatedDomainEventNotifier(
    NotificationOrchestrator notificationOrchestrator) : IDomainEventNotifier<MissionCreatedDomainEvent>
{
    public async Task HandleAsync(
        MissionCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await notificationOrchestrator.NotifyMissionCreatedAsync(
            domainEvent.MissionId,
            domainEvent.OrganizationId,
            domainEvent.MissionName,
            domainEvent.ActorEmployeeId,
            cancellationToken);
    }
}

public sealed class MissionUpdatedDomainEventNotifier(
    NotificationOrchestrator notificationOrchestrator) : IDomainEventNotifier<MissionUpdatedDomainEvent>
{
    public async Task HandleAsync(
        MissionUpdatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await notificationOrchestrator.NotifyMissionUpdatedAsync(
            domainEvent.MissionId,
            domainEvent.OrganizationId,
            domainEvent.MissionName,
            domainEvent.ActorEmployeeId,
            cancellationToken);
    }
}

public sealed class MissionDeletedDomainEventNotifier(
    NotificationOrchestrator notificationOrchestrator) : IDomainEventNotifier<MissionDeletedDomainEvent>
{
    public async Task HandleAsync(
        MissionDeletedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await notificationOrchestrator.NotifyMissionDeletedAsync(
            domainEvent.MissionId,
            domainEvent.OrganizationId,
            domainEvent.MissionName,
            domainEvent.ActorEmployeeId,
            cancellationToken);
    }
}

public sealed class CheckinCreatedDomainEventNotifier(
    NotificationOrchestrator notificationOrchestrator) : IDomainEventNotifier<CheckinCreatedDomainEvent>
{
    public async Task HandleAsync(
        CheckinCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await notificationOrchestrator.NotifyCheckinCreatedAsync(
            domainEvent.CheckinId,
            domainEvent.IndicatorId,
            domainEvent.OrganizationId,
            domainEvent.EmployeeId,
            domainEvent.IndicatorName,
            cancellationToken);
    }
}
