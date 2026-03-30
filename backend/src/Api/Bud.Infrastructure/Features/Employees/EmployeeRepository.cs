using Bud.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

using Bud.Shared.Contracts;

namespace Bud.Infrastructure.Features.Employees;

public sealed class EmployeeRepository(ApplicationDbContext dbContext) : IEmployeeRepository
{
    public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Employees.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Employee?> GetByIdWithEmployeeTeamsAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Employees.Include(c => c.EmployeeTeams).FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<PagedResult<Employee>> GetAllAsync(
        Guid? teamId, string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var query = dbContext.Employees.AsNoTracking();

        if (teamId.HasValue)
        {
            var teamEmployeeIds = dbContext.EmployeeTeams
                .Where(ct2 => ct2.TeamId == teamId.Value)
                .Select(ct2 => ct2.EmployeeId);
            query = query.Where(c => teamEmployeeIds.Contains(c.Id));
        }

        query = new EmployeeSearchSpecification(search, dbContext.Database.IsNpgsql()).Apply(query);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Employee> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<List<Employee>> GetLeadersAsync(Guid? organizationId, CancellationToken ct = default)
    {
        var query = dbContext.Employees
            .AsNoTracking()
            .Include(c => c.Organization)
            .Where(c => c.Role == EmployeeRole.Leader);

        if (organizationId.HasValue)
        {
            query = query.Where(c => c.OrganizationId == organizationId.Value);
        }

        return await query
            .OrderBy(c => c.FullName)
            .ToListAsync(ct);
    }

    public async Task<List<Employee>> GetSubordinatesAsync(
        Guid employeeId, int maxDepth, CancellationToken ct = default)
    {
        var allSubordinates = await dbContext.Employees
            .AsNoTracking()
            .Where(c => c.LeaderId != null)
            .Include(c => c.Leader)
            .ToListAsync(ct);

        var childrenByLeader = allSubordinates
            .GroupBy(c => c.LeaderId!.Value)
            .ToDictionary(group => group.Key, group => group.OrderBy(c => c.FullName).ToList());

        var result = new List<Employee>();
        CollectSubordinates(employeeId, 0, maxDepth, childrenByLeader, result);
        return result;
    }

    public async Task<List<Team>> GetTeamsAsync(Guid employeeId, CancellationToken ct = default)
    {
        return await dbContext.EmployeeTeams
            .AsNoTracking()
            .Where(ct2 => ct2.EmployeeId == employeeId)
            .Include(ct2 => ct2.Team)
            .Select(ct2 => ct2.Team)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<List<Team>> GetEligibleTeamsForAssignmentAsync(
        Guid employeeId, Guid organizationId, string? search, int limit, CancellationToken ct = default)
    {
        var currentTeamIds = await dbContext.EmployeeTeams
            .Where(ct2 => ct2.EmployeeId == employeeId)
            .Select(ct2 => ct2.TeamId)
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
        var query = dbContext.Employees.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = new EmployeeSearchSpecification(search, dbContext.Database.IsNpgsql()).Apply(query);
        }

        return await query
            .OrderBy(c => c.FullName)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Employees.AnyAsync(c => c.Id == id, ct);

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId, CancellationToken ct = default)
    {
        var query = dbContext.Employees.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return !await query.AnyAsync(c => c.Email == email, ct);
    }

    public async Task<bool> HasSubordinatesAsync(Guid employeeId, CancellationToken ct = default)
        => await dbContext.Employees.AnyAsync(c => c.LeaderId == employeeId, ct);

    public async Task<bool> HasMissionsAsync(Guid employeeId, CancellationToken ct = default)
        => await dbContext.Missions.AnyAsync(m => m.EmployeeId == employeeId, ct);

    public async Task<int> CountTeamsByIdsAndOrganizationAsync(List<Guid> teamIds, Guid organizationId, CancellationToken ct = default)
        => await dbContext.Teams.CountAsync(t => teamIds.Contains(t.Id) && t.OrganizationId == organizationId, ct);

    public async Task<int> CountByIdsAndOrganizationAsync(List<Guid> ids, Guid organizationId, CancellationToken ct = default)
        => await dbContext.Employees.CountAsync(c => ids.Contains(c.Id) && c.OrganizationId == organizationId, ct);

    public async Task<bool> IsValidLeaderAsync(Guid leaderId, Guid? requiredOrganizationId, CancellationToken ct = default)
    {
        var leader = await dbContext.Employees
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == leaderId, ct);

        if (leader is null || leader.Role != EmployeeRole.Leader)
        {
            return false;
        }

        return !requiredOrganizationId.HasValue || leader.OrganizationId == requiredOrganizationId.Value;
    }

    public async Task AddAsync(Employee entity, CancellationToken ct = default)
        => await dbContext.Employees.AddAsync(entity, ct);

    public async Task RemoveAsync(Employee entity, CancellationToken ct = default)
        => await Task.FromResult(dbContext.Employees.Remove(entity));

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await dbContext.SaveChangesAsync(ct);

    private static void CollectSubordinates(
        Guid leaderId,
        int depth,
        int maxDepth,
        IReadOnlyDictionary<Guid, List<Employee>> childrenByLeader,
        List<Employee> acc)
    {
        if (depth >= maxDepth || !childrenByLeader.TryGetValue(leaderId, out var children))
        {
            return;
        }

        foreach (var child in children)
        {
            acc.Add(child);
            CollectSubordinates(child.Id, depth + 1, maxDepth, childrenByLeader, acc);
        }
    }
}
