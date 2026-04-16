using Bud.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure.Features.Notifications;

public sealed class NotificationRecipientResolver(ApplicationDbContext dbContext) : INotificationRecipientResolver
{
    public async Task<List<Guid>> ResolveMissionRecipientsAsync(
        Guid missionId,
        Guid organizationId,
        Guid? excludeEmployeeId = null,
        CancellationToken cancellationToken = default)
    {
        var mission = await dbContext.Missions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == missionId && g.OrganizationId == organizationId, cancellationToken);

        if (mission is null)
        {
            return [];
        }

        List<Guid> recipientIds;

        if (mission.EmployeeId.HasValue)
        {
            // Responsible employee + their leader
            recipientIds = [mission.EmployeeId.Value];

            var leaderId = await dbContext.Memberships
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(m => m.EmployeeId == mission.EmployeeId.Value && m.OrganizationId == organizationId)
                .Select(m => m.LeaderId)
                .FirstOrDefaultAsync(cancellationToken);

            if (leaderId.HasValue)
            {
                recipientIds.Add(leaderId.Value);
            }
        }
        else
        {
            // No responsible: notify all employees in the org
            recipientIds = await dbContext.Memberships
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(m => m.OrganizationId == organizationId)
                .Select(m => m.EmployeeId)
                .ToListAsync(cancellationToken);
        }

        if (excludeEmployeeId.HasValue)
        {
            recipientIds.Remove(excludeEmployeeId.Value);
        }

        return recipientIds.Distinct().ToList();
    }

    public async Task<Guid?> ResolveMissionIdFromIndicatorAsync(
        Guid indicatorId,
        CancellationToken cancellationToken = default)
    {
        var missionId = await dbContext.Indicators
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(i => i.Id == indicatorId)
            .Select(i => i.MissionId)
            .FirstOrDefaultAsync(cancellationToken);

        return missionId == Guid.Empty ? null : missionId;
    }

    public async Task<string?> ResolveEmployeeNameAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Employees
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(e => e.Id == employeeId)
            .Select(e => e.FullName)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
