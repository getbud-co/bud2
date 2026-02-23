
namespace Bud.Server.Domain.Events;

public sealed record MetricCheckinCreatedDomainEvent(
    Guid CheckinId,
    Guid MetricId,
    Guid OrganizationId,
    Guid? CreatorCollaboratorId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
