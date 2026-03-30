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

            var leader = await dbContext.Employees
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => c.Id == mission.EmployeeId.Value && c.OrganizationId == organizationId)
                .Select(c => c.LeaderId)
                .FirstOrDefaultAsync(cancellationToken);

            if (leader.HasValue)
            {
                recipientIds.Add(leader.Value);
            }
        }
        else
        {
            // No responsible: notify all employees in the org
            recipientIds = await dbContext.Employees
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => c.OrganizationId == organizationId)
                .Select(c => c.Id)
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
            .Where(c => c.Id == employeeId)
            .Select(c => c.FullName)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
