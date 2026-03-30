namespace Bud.Domain.Indicators.Events;

public sealed record CheckinCreatedDomainEvent(
    Guid CheckinId,
    Guid IndicatorId,
    Guid OrganizationId,
    Guid EmployeeId,
    string IndicatorName) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
