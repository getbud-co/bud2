using Bud.Application.Ports;
using Bud.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure.Features.Me;

public sealed class TenantAuthorizationService(
    ApplicationDbContext dbContext,
    ITenantProvider tenantProvider) : ITenantAuthorizationService
{
    public async Task<bool> UserBelongsToTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantProvider.IsGlobalAdmin)
        {
            return true;
        }

        if (tenantProvider.EmployeeId is null)
        {
            return false;
        }

        return await dbContext.OrganizationEmployeeMembers
            .AsNoTracking()
            .IgnoreQueryFilters()
            .AnyAsync(
                m => m.EmployeeId == tenantProvider.EmployeeId.Value && m.OrganizationId == tenantId,
                cancellationToken);
    }

    public async Task<List<Guid>> GetUserTenantIdsAsync(CancellationToken cancellationToken = default)
    {
        if (tenantProvider.IsGlobalAdmin)
        {
            return await dbContext.Organizations
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Select(o => o.Id)
                .ToListAsync(cancellationToken);
        }

        if (tenantProvider.EmployeeId is null)
        {
            return [];
        }

        return await dbContext.OrganizationEmployeeMembers
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(m => m.EmployeeId == tenantProvider.EmployeeId.Value)
            .Select(m => m.OrganizationId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
