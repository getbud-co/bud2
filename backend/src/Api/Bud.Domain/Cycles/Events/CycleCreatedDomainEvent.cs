namespace Bud.Domain.Cycles.Events;

public sealed record CycleCreatedDomainEvent(
    Guid CycleId,
    Guid OrganizationId,
    string CycleName,
    Guid? ActorCollaboratorId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
