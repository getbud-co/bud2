using Bud.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure.Features.Organizations;

public sealed class OrganizationRepository(ApplicationDbContext dbContext) : IOrganizationRepository
{
    public async Task<Organization?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<PagedResult<Organization>> GetAllAsync(
        string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var query = dbContext.Organizations.AsNoTracking();
        query = new OrganizationSearchSpecification(search, dbContext.Database.IsNpgsql()).Apply(query);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(o => EF.Property<string>(o, nameof(Organization.Name)))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Organization>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> ExistsByNameAsync(OrganizationDomainName name, Guid? excludeId = null, CancellationToken ct = default)
    {
        return await dbContext.Organizations
            .IgnoreQueryFilters()
            .Where(o => !excludeId.HasValue || o.Id != excludeId.Value)
            .AnyAsync(o => EF.Property<string>(o, nameof(Organization.Name)) == name.Value, ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Organizations.AnyAsync(o => o.Id == id, ct);

    public async Task<bool> HasEmployeesAsync(Guid organizationId, CancellationToken ct = default)
        => await dbContext.Employees.AnyAsync(c => c.OrganizationId == organizationId, ct);

    public async Task AddAsync(Organization entity, CancellationToken ct = default)
        => await dbContext.Organizations.AddAsync(entity, ct);

    public Task RemoveAsync(Organization entity, CancellationToken ct = default)
    {
        dbContext.Organizations.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await dbContext.SaveChangesAsync(ct);
}
