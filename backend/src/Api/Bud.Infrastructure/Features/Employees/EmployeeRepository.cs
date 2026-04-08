using Bud.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

using Bud.Shared.Contracts;

namespace Bud.Infrastructure.Features.Employees;

public sealed class EmployeeRepository(ApplicationDbContext dbContext) : IEmployeeRepository
{
    public async Task<OrganizationEmployeeMember?> GetByIdAsync(Guid employeeId, CancellationToken ct = default)
        => await dbContext.OrganizationEmployeeMembers
            .Include(m => m.Employee)
            .Include(m => m.Team)
            .Include(m => m.Organization)
            .FirstOrDefaultAsync(m => m.EmployeeId == employeeId, ct);

    public async Task<OrganizationEmployeeMember?> GetByIdWithEmployeeTeamsAsync(Guid employeeId, CancellationToken ct = default)
        => await dbContext.OrganizationEmployeeMembers
            .Include(m => m.Employee)
                .ThenInclude(e => e.EmployeeTeams)
            .FirstOrDefaultAsync(m => m.EmployeeId == employeeId, ct);

    public async Task<PagedResult<OrganizationEmployeeMember>> GetAllAsync(
        Guid? teamId, string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var query = dbContext.OrganizationEmployeeMembers
            .Include(m => m.Employee)
            .AsNoTracking();

        if (teamId.HasValue)
        {
            var teamEmployeeIds = dbContext.EmployeeTeams
                .Where(et => et.TeamId == teamId.Value)
                .Select(et => et.EmployeeId);
            query = query.Where(m => teamEmployeeIds.Contains(m.EmployeeId));
        }

        query = new EmployeeSearchSpecification(search, dbContext.Database.IsNpgsql()).Apply(query);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(m => m.Employee.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<OrganizationEmployeeMember> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<List<OrganizationEmployeeMember>> GetLeadersAsync(Guid? organizationId, CancellationToken ct = default)
    {
        var query = dbContext.OrganizationEmployeeMembers
            .AsNoTracking()
            .Include(m => m.Employee)
            .Include(m => m.Team)
            .Include(m => m.Organization)
            .Where(m => m.Role == EmployeeRole.Leader);

        if (organizationId.HasValue)
        {
            query = query.Where(m => m.OrganizationId == organizationId.Value);
        }

        return await query
            .OrderBy(m => m.Employee.FullName)
            .ToListAsync(ct);
    }

    public async Task<List<OrganizationEmployeeMember>> GetSubordinatesAsync(
        Guid employeeId, int maxDepth, CancellationToken ct = default)
    {
        var allMembers = await dbContext.OrganizationEmployeeMembers
            .AsNoTracking()
            .Include(m => m.Employee)
            .Where(m => m.LeaderId != null)
            .ToListAsync(ct);

        var childrenByLeader = allMembers
            .GroupBy(m => m.LeaderId!.Value)
            .ToDictionary(group => group.Key, group => group.OrderBy(m => m.Employee.FullName).ToList());

        var result = new List<OrganizationEmployeeMember>();
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

    public async Task<List<OrganizationEmployeeMember>> GetLookupAsync(string? search, int limit, CancellationToken ct = default)
    {
        var query = dbContext.OrganizationEmployeeMembers
            .AsNoTracking()
            .Include(m => m.Employee);

        if (!string.IsNullOrWhiteSpace(search))
        {
            return await new EmployeeSearchSpecification(search, dbContext.Database.IsNpgsql())
                .Apply(query)
                .OrderBy(m => m.Employee.FullName)
                .Take(limit)
                .ToListAsync(ct);
        }

        return await query
            .OrderBy(m => m.Employee.FullName)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid employeeId, CancellationToken ct = default)
        => await dbContext.OrganizationEmployeeMembers.AnyAsync(m => m.EmployeeId == employeeId, ct);

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
        => await dbContext.OrganizationEmployeeMembers.AnyAsync(m => m.LeaderId == employeeId, ct);

    public async Task<bool> HasMissionsAsync(Guid employeeId, CancellationToken ct = default)
        => await dbContext.Missions.AnyAsync(m => m.EmployeeId == employeeId, ct);

    public async Task<int> CountTeamsByIdsAndOrganizationAsync(List<Guid> teamIds, Guid organizationId, CancellationToken ct = default)
        => await dbContext.Teams.CountAsync(t => teamIds.Contains(t.Id) && t.OrganizationId == organizationId, ct);

    public async Task<int> CountByIdsAndOrganizationAsync(List<Guid> ids, Guid organizationId, CancellationToken ct = default)
        => await dbContext.OrganizationEmployeeMembers.CountAsync(m => ids.Contains(m.EmployeeId) && m.OrganizationId == organizationId, ct);

    public async Task<bool> IsValidLeaderAsync(Guid leaderId, Guid? requiredOrganizationId, CancellationToken ct = default)
    {
        var member = await dbContext.OrganizationEmployeeMembers
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.EmployeeId == leaderId, ct);

        if (member is null || member.Role != EmployeeRole.Leader)
        {
            return false;
        }

        return !requiredOrganizationId.HasValue || member.OrganizationId == requiredOrganizationId.Value;
    }

    public async Task AddAsync(Employee employee, OrganizationEmployeeMember member, CancellationToken ct = default)
    {
        await dbContext.Employees.AddAsync(employee, ct);
        await dbContext.OrganizationEmployeeMembers.AddAsync(member, ct);
    }

    public async Task RemoveAsync(OrganizationEmployeeMember member, CancellationToken ct = default)
        => await Task.FromResult(dbContext.OrganizationEmployeeMembers.Remove(member));

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await dbContext.SaveChangesAsync(ct);

    private static void CollectSubordinates(
        Guid leaderId,
        int depth,
        int maxDepth,
        IReadOnlyDictionary<Guid, List<OrganizationEmployeeMember>> childrenByLeader,
        List<OrganizationEmployeeMember> acc)
    {
        if (depth >= maxDepth || !childrenByLeader.TryGetValue(leaderId, out var children))
        {
            return;
        }

        foreach (var child in children)
        {
            acc.Add(child);
            CollectSubordinates(child.EmployeeId, depth + 1, maxDepth, childrenByLeader, acc);
        }
    }
}
