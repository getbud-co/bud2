namespace Bud.Application.Features.Notifications;

public interface INotificationRecipientResolver
{
    Task<List<Guid>> ResolveMissionRecipientsAsync(
        Guid missionId,
        Guid organizationId,
        Guid? excludeEmployeeId = null,
        CancellationToken cancellationToken = default);

    Task<Guid?> ResolveMissionIdFromIndicatorAsync(
        Guid indicatorId,
        CancellationToken cancellationToken = default);

    Task<string?> ResolveEmployeeNameAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);
}
