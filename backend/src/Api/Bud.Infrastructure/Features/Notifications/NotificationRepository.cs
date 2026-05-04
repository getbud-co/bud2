using Bud.Application.Common;
using Bud.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure.Features.Notifications;

public sealed class NotificationRepository(ApplicationDbContext dbContext) : INotificationRepository
{
    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == id, ct);

    public async Task<PagedResult<Notification>> GetByRecipientAsync(Guid recipientId, bool? isRead, int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = PaginationNormalizer.Normalize(page, pageSize);

        var query = dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.RecipientEmployeeId == recipientId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .AsQueryable();

        if (isRead.HasValue)
        {
            query = query.Where(n => n.IsRead == isRead.Value).OrderByDescending(n => n.CreatedAtUtc);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Notification>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public Task<List<Notification>> GetUnreadByRecipientAsync(Guid recipientId, CancellationToken ct = default)
    {
        return dbContext.Notifications
            .Where(n => n.RecipientEmployeeId == recipientId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken ct = default)
        => dbContext.Notifications.AddRangeAsync(notifications, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => dbContext.SaveChangesAsync(ct);
}
