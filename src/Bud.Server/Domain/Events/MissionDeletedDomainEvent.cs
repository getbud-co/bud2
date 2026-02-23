
namespace Bud.Server.Domain.Events;

public sealed record MissionDeletedDomainEvent(
    Guid MissionId,
    Guid OrganizationId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
