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
        if (!EmailAddress.TryCreate(email, out var emailAddress))
        {
            return Result<List<OrganizationSnapshot>>.Failure("E-mail é obrigatório.");
        }

        var employee = await dbContext.Employees
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Email == emailAddress.Value, cancellationToken);

        if (employee?.IsGlobalAdmin == true)
        {
            var allOrgs = await dbContext.Organizations
                .AsNoTracking()
                .IgnoreQueryFilters()
                .OrderBy(o => o.Name)
                .Select(o => new OrganizationSnapshot
                {
                    Id = o.Id,
                    Name = o.Name
                })
                .ToListAsync(cancellationToken);

            return Result<List<OrganizationSnapshot>>.Success(allOrgs);
        }

        var orgsFromMembership = await dbContext.Employees
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(c => c.Email == emailAddress.Value)
            .Include(c => c.Organization)
            .Select(c => new OrganizationSnapshot
            {
                Id = c.Organization.Id,
                Name = c.Organization.Name
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
