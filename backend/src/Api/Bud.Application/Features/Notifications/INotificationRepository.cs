namespace Bud.Application.Features.Notifications;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Notification>> GetByRecipientAsync(Guid recipientId, bool? isRead, int page, int pageSize, CancellationToken ct = default);
    Task<List<Notification>> GetUnreadByRecipientAsync(Guid recipientId, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
