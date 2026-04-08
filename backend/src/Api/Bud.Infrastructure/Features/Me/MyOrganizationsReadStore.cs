using Bud.Application.Common;
using Bud.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure.Features.Me;

public sealed class MyOrganizationsReadStore(ApplicationDbContext dbContext) : IMyOrganizationsReadStore
{
    public async Task<Result<List<OrganizationSnapshot>>> GetMyOrganizationsAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return Result<List<OrganizationSnapshot>>.Failure("E-mail é obrigatório.");
        }

        var member = await dbContext.OrganizationEmployeeMembers
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(m => m.Employee)
            .FirstOrDefaultAsync(m => m.Employee.Email == normalizedEmail, cancellationToken);

        if (member?.IsGlobalAdmin == true)
        {
            var allOrgs = await dbContext.Organizations
                .AsNoTracking()
                .IgnoreQueryFilters()
                .OrderBy(o => o.Name)
                .Select(o => new OrganizationSnapshot
                {
                    Id = o.Id,
                    Name = o.Name,
                })
                .ToListAsync(cancellationToken);

            return Result<List<OrganizationSnapshot>>.Success(allOrgs);
        }

        var orgsFromMembership = await dbContext.OrganizationEmployeeMembers
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(m => m.Employee.Email == normalizedEmail)
            .Include(m => m.Organization)
            .Select(m => new OrganizationSnapshot
            {
                Id = m.Organization.Id,
                Name = m.Organization.Name,
            })
            .ToListAsync(cancellationToken);

        var organizations = orgsFromMembership
            .GroupBy(o => o.Id)
            .Select(g => g.First())
            .OrderBy(o => o.Name)
            .ToList();

        return Result<List<OrganizationSnapshot>>.Success(organizations);
    }
}
