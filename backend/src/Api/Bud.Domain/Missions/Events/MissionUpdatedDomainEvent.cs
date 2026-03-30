namespace Bud.Domain.Missions.Events;

public sealed record MissionUpdatedDomainEvent(
    Guid MissionId,
    Guid OrganizationId,
    string MissionName,
    Guid? ActorEmployeeId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
