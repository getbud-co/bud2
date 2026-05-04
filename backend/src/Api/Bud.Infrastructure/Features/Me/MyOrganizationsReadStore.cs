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
            .Include(e => e.Memberships)
                .ThenInclude(m => m.Organization)
            .FirstOrDefaultAsync(e => e.Email == normalizedEmail, cancellationToken);

        if (employee is null)
        {
            return Result<List<OrganizationSnapshot>>.Success([]);
        }

        if (employee.Memberships.Any(m => m.IsGlobalAdmin))
        {
            var allOrgs = await dbContext.Organizations
                .AsNoTracking()
                .IgnoreQueryFilters()
                .OrderBy(o => EF.Property<string>(o, nameof(Organization.Name)))
                .Select(o => new OrganizationSnapshot
                {
                    Id = o.Id,
                    Name = o.Name,
                })
                .ToListAsync(cancellationToken);

            return Result<List<OrganizationSnapshot>>.Success(allOrgs);
        }

        var organizations = employee.Memberships
            .Select(m => new OrganizationSnapshot
            {
                Id = m.Organization.Id,
                Name = m.Organization.Name,
            })
            .GroupBy(o => o.Id)
            .Select(g => g.First())
            .OrderBy(o => o.Name)
            .ToList();

        return Result<List<OrganizationSnapshot>>.Success(organizations);
    }
}
