using Bud.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure.Features.Sessions;

public sealed class TenantAuthorizationService(
    ApplicationDbContext dbContext,
    ITenantProvider tenantProvider) : ITenantAuthorizationService
{
    public async Task<bool> UserBelongsToTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var userEmail = TryNormalizeEmail(tenantProvider.UserEmail);
        if (userEmail is null)
        {
            return false;
        }

        var employee = await dbContext.Employees
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Email == userEmail, cancellationToken);

        if (employee?.IsGlobalAdmin == true)
        {
            return await dbContext.Organizations
                .AsNoTracking()
                .IgnoreQueryFilters()
                .AnyAsync(o => o.Id == tenantId, cancellationToken);
        }

        return await dbContext.Employees
            .AsNoTracking()
            .IgnoreQueryFilters()
            .AnyAsync(c => c.Email == userEmail && c.OrganizationId == tenantId, cancellationToken);
    }

    public async Task<List<Guid>> GetUserTenantIdsAsync(CancellationToken cancellationToken = default)
    {
        var userEmail = TryNormalizeEmail(tenantProvider.UserEmail);
        if (userEmail is null)
        {
            return [];
        }

        var employee = await dbContext.Employees
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Email == userEmail, cancellationToken);

        if (employee?.IsGlobalAdmin == true)
        {
            return await dbContext.Organizations
                .AsNoTracking()
                .IgnoreQueryFilters()
                .OrderBy(o => o.Name)
                .Select(o => o.Id)
                .ToListAsync(cancellationToken);
        }

        return await dbContext.Employees
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(c => c.Email == userEmail)
            .Select(c => c.OrganizationId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private static string? TryNormalizeEmail(string? email)
        => EmailAddress.TryCreate(email, out var emailAddress) ? emailAddress.Value : null;
}
