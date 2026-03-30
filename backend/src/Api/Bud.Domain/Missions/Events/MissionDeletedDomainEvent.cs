namespace Bud.Domain.Missions.Events;

public sealed record MissionDeletedDomainEvent(
    Guid MissionId,
    Guid OrganizationId,
    string MissionName,
    Guid? ActorEmployeeId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
