using Bud.Infrastructure.Persistence;
using Bud.Application.Ports;
using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure.Authorization;

public sealed class TenantAuthorizationService(
    ApplicationDbContext dbContext,
    ITenantProvider tenantProvider) : ITenantAuthorizationService
{
    public async Task<bool> UserBelongsToTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantProvider.IsGlobalAdmin)
        {
            return await dbContext.Organizations
                .AnyAsync(o => o.Id == tenantId, cancellationToken);
        }

        if (string.IsNullOrEmpty(tenantProvider.UserEmail))
        {
            return false;
        }

        // Verificar se é colaborador da organização
        return await dbContext.OrganizationEmployeeMembers
            .AnyAsync(m =>
                m.OrganizationId == tenantId &&
                m.Employee.Email == tenantProvider.UserEmail,
                cancellationToken);
    }

    public async Task<List<Guid>> GetUserTenantIdsAsync(CancellationToken cancellationToken = default)
    {
        if (tenantProvider.IsGlobalAdmin)
        {
            return await dbContext.Organizations.Select(o => o.Id).ToListAsync(cancellationToken);
        }

        if (string.IsNullOrEmpty(tenantProvider.UserEmail))
        {
            return [];
        }

        // Organizações onde o usuário é colaborador
        var employeeOrgIds = await dbContext.OrganizationEmployeeMembers
            .Where(m => m.Employee.Email == tenantProvider.UserEmail)
            .Select(m => m.OrganizationId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return employeeOrgIds.Distinct().ToList();
    }
}
