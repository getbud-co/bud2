using Bud.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure.Features.Employees;

public sealed class EmployeeRepository(ApplicationDbContext dbContext) : IEmployeeRepository
{
    // Membership has a tenant query filter (OrganizationId == _tenantId).
    // All Include(e => e.Memberships) calls below are therefore deterministic:
    // when a tenant is selected the collection contains exactly one entry — the current org's membership.

    public async Task<Employee?> GetByIdAsync(Guid employeeId, CancellationToken ct = default)
        => await dbContext.Employees
            .Include(e => e.Memberships)
            .FirstOrDefaultAsync(e => e.Id == employeeId, ct);

    public async Task<Employee?> GetByIdWithEmployeeTeamsAsync(Guid employeeId, CancellationToken ct = default)
        => await dbContext.Employees
            .Include(e => e.Memberships)
            .Include(e => e.EmployeeTeams)
                .ThenInclude(et => et.Team)
            .FirstOrDefaultAsync(e => e.Id == employeeId, ct);

    public async Task<PagedResult<Employee>> GetAllAsync(
        Guid? teamId, string? search, int page, int pageSize, CancellationToken ct = default)
    {
        // Only return employees that have a membership visible in the current tenant context.
        var query = dbContext.Employees
            .Include(e => e.Memberships)
            .AsNoTracking()
            .Where(e => e.Memberships.Any());

        if (teamId.HasValue)
        {
            var teamEmployeeIds = dbContext.EmployeeTeams
                .IgnoreQueryFilters()
                .Where(et => et.TeamId == teamId.Value)
                .Select(et => et.EmployeeId);
            query = query.Where(e => teamEmployeeIds.Contains(e.Id));
        }

        query = new EmployeeSearchSpecification(search, dbContext.Database.IsNpgsql()).Apply(query);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(e => e.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // Load teams separately to avoid translation issues with EmployeeTeam's global query filter.
        var employeeIds = items.Select(e => e.Id).ToList();
        var teamByEmployee = await dbContext.EmployeeTeams
            .IgnoreQueryFilters()
            .Where(et => employeeIds.Contains(et.EmployeeId))
            .Include(et => et.Team)
            .AsNoTracking()
            .ToListAsync(ct);

        var teamsByEmployeeId = teamByEmployee
            .GroupBy(et => et.EmployeeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var employee in items)
        {
            if (teamsByEmployeeId.TryGetValue(employee.Id, out var employeeTeams))
            {
                employee.EmployeeTeams = employeeTeams;
            }
        }

        return new PagedResult<Employee> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<List<Employee>> GetSubordinatesAsync(
        Guid employeeId, int maxDepth, CancellationToken ct = default)
    {
        var allMembers = await dbContext.Memberships
            .AsNoTracking()
            .Include(m => m.Employee)
                .ThenInclude(e => e.Memberships)
            .Where(m => m.LeaderId != null)
            .ToListAsync(ct);

        var childrenByLeader = allMembers
            .GroupBy(m => m.LeaderId!.Value)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(m => m.Employee.FullName).Select(m => m.Employee).ToList());

        var result = new List<Employee>();
        CollectSubordinates(employeeId, 0, maxDepth, childrenByLeader, result);
        return result;
    }

    public async Task<List<Team>> GetTeamsAsync(Guid employeeId, CancellationToken ct = default)
    {
        return await dbContext.EmployeeTeams
            .AsNoTracking()
            .Where(et => et.EmployeeId == employeeId)
            .Include(et => et.Team)
            .Select(et => et.Team)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<List<Team>> GetEligibleTeamsForAssignmentAsync(
        Guid employeeId, Guid organizationId, string? search, int limit, CancellationToken ct = default)
    {
        var currentTeamIds = await dbContext.EmployeeTeams
            .Where(et => et.EmployeeId == employeeId)
            .Select(et => et.TeamId)
            .ToListAsync(ct);

        var query = dbContext.Teams
            .AsNoTracking()
            .Where(t => t.OrganizationId == organizationId)
            .Where(t => !currentTeamIds.Contains(t.Id));

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = new TeamSearchSpecification(search, dbContext.Database.IsNpgsql()).Apply(query);
        }

        return await query
            .OrderBy(t => t.Name)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<List<Employee>> GetLookupAsync(string? search, int limit, CancellationToken ct = default)
    {
        var query = dbContext.Employees
            .Include(e => e.Memberships)
            .AsNoTracking()
            .Where(e => e.Memberships.Any());

        if (!string.IsNullOrWhiteSpace(search))
        {
            return await new EmployeeSearchSpecification(search, dbContext.Database.IsNpgsql())
                .Apply(query)
                .OrderBy(e => e.FullName)
                .Take(limit)
                .ToListAsync(ct);
        }

        return await query
            .OrderBy(e => e.FullName)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<List<Employee>> GetLeadersAsync(Guid? organizationId = null, CancellationToken ct = default)
    {
        var query = dbContext.Employees
            .AsNoTracking()
            .Include(e => e.Memberships)
                .ThenInclude(m => m.Organization)
            .Where(e => e.Memberships.Any(m => m.Role == EmployeeRole.TeamLeader));

        if (organizationId.HasValue)
        {
            query = query.Where(e => e.Memberships.Any(m => m.OrganizationId == organizationId.Value));
        }

        return await query.OrderBy(e => e.FullName).ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid employeeId, CancellationToken ct = default)
        => await dbContext.Memberships.AnyAsync(m => m.EmployeeId == employeeId, ct);

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId, CancellationToken ct = default)
    {
        var query = dbContext.Employees.AsQueryable();

        if (excludeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeId.Value);
        }

        return !await query.AnyAsync(e => e.Email == email, ct);
    }

    public async Task<bool> HasSubordinatesAsync(Guid employeeId, CancellationToken ct = default)
        => await dbContext.Memberships.AnyAsync(m => m.LeaderId == employeeId, ct);

    public async Task<bool> HasMissionsAsync(Guid employeeId, CancellationToken ct = default)
        => await dbContext.Missions.AnyAsync(m => m.EmployeeId == employeeId, ct);

    public async Task<int> CountTeamsByIdsAndOrganizationAsync(List<Guid> teamIds, Guid organizationId, CancellationToken ct = default)
        => await dbContext.Teams.CountAsync(t => teamIds.Contains(t.Id) && t.OrganizationId == organizationId, ct);

    public async Task<int> CountByIdsAndOrganizationAsync(List<Guid> ids, Guid organizationId, CancellationToken ct = default)
        => await dbContext.Memberships.CountAsync(m => ids.Contains(m.EmployeeId) && m.OrganizationId == organizationId, ct);

    public async Task<bool> IsValidLeaderAsync(Guid leaderId, Guid? requiredOrganizationId, CancellationToken ct = default)
    {
        var member = await dbContext.Memberships
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.EmployeeId == leaderId, ct);

        if (member is null || member.Role != EmployeeRole.TeamLeader)
        {
            return false;
        }

        return !requiredOrganizationId.HasValue || member.OrganizationId == requiredOrganizationId.Value;
    }

    public async Task AddAsync(Employee employee, CancellationToken ct = default)
        => await dbContext.Employees.AddAsync(employee, ct);

    public async Task RemoveAsync(Employee employee, CancellationToken ct = default)
        => await Task.FromResult(dbContext.Employees.Remove(employee));

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await dbContext.SaveChangesAsync(ct);

    private static void CollectSubordinates(
        Guid leaderId,
        int depth,
        int maxDepth,
        IReadOnlyDictionary<Guid, List<Employee>> childrenByLeader,
        List<Employee> acc)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var normalized = $"%{search.Trim()}%";
        return query.Where(c =>
            EF.Functions.ILike(EF.Property<string>(c, nameof(Employee.FullName)), normalized) ||
            EF.Functions.ILike(EF.Property<string>(c, nameof(Employee.Email)), normalized));
    }
}
