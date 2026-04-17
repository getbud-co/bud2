namespace Bud.Application.Features.Notifications.UseCases;

public sealed class ListNotifications(
    INotificationRepository notificationRepository,
    ITenantProvider tenantProvider)
{
    public async Task<Result<PagedResult<Notification>>> ExecuteAsync(
        bool? isRead,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (!tenantProvider.EmployeeId.HasValue)
        {
            return Result<PagedResult<Notification>>.Forbidden("Funcionário não identificado.");
        }

        (page, pageSize) = PaginationNormalizer.Normalize(page, pageSize);
        var notifications = await notificationRepository.GetByRecipientAsync(
            tenantProvider.EmployeeId.Value,
            isRead,
            page,
            pageSize,
            cancellationToken);

        return Result<PagedResult<Notification>>.Success(notifications);
    }
}
