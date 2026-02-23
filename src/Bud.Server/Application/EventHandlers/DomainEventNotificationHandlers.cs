using Bud.Server.Domain.Events;

namespace Bud.Server.Application.EventHandlers;

public sealed class MissionCreatedDomainEventHandler(
    NotificationOrchestrator notificationOrchestrator) : IDomainEventHandler<MissionCreatedDomainEvent>
{
    public async Task HandleAsync(
        MissionCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await notificationOrchestrator.NotifyMissionCreatedAsync(
            domainEvent.MissionId,
            domainEvent.OrganizationId,
            cancellationToken);
    }
}

public sealed class MissionUpdatedDomainEventHandler(
    NotificationOrchestrator notificationOrchestrator) : IDomainEventHandler<MissionUpdatedDomainEvent>
{
    public async Task HandleAsync(
        MissionUpdatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await notificationOrchestrator.NotifyMissionUpdatedAsync(
            domainEvent.MissionId,
            domainEvent.OrganizationId,
            cancellationToken);
    }
}

public sealed class MissionDeletedDomainEventHandler(
    NotificationOrchestrator notificationOrchestrator) : IDomainEventHandler<MissionDeletedDomainEvent>
{
    public async Task HandleAsync(
        MissionDeletedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await notificationOrchestrator.NotifyMissionDeletedAsync(
            domainEvent.MissionId,
            domainEvent.OrganizationId,
            cancellationToken);
    }
}

public sealed class MetricCheckinCreatedDomainEventHandler(
    NotificationOrchestrator notificationOrchestrator) : IDomainEventHandler<MetricCheckinCreatedDomainEvent>
{
    public async Task HandleAsync(
        MetricCheckinCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await notificationOrchestrator.NotifyMetricCheckinCreatedAsync(
            domainEvent.CheckinId,
            domainEvent.MetricId,
            domainEvent.OrganizationId,
            domainEvent.CreatorCollaboratorId,
            cancellationToken);
    }
}
