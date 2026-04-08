using Bud.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

using Bud.Shared.Contracts;

namespace Bud.Infrastructure.Features.Teams;

public sealed class TeamRepository(ApplicationDbContext dbContext) : ITeamRepository
{
    public async Task<Team?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Teams
            .AsNoTracking()
            .Include(t => t.EmployeeTeams).ThenInclude(et => et.Employee)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<Team?> GetByIdWithEmployeeTeamsAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Teams
            .Include(t => t.EmployeeTeams).ThenInclude(et => et.Employee)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<PagedResult<Team>> GetAllAsync(
        Guid? parentTeamId, string? search, int page, int pageSize, CancellationToken ct = default)
    {
        IQueryable<Team> query = dbContext.Teams.AsNoTracking()
            .Include(t => t.EmployeeTeams).ThenInclude(et => et.Employee);

        if (parentTeamId.HasValue)
        {
            query = query.Where(t => t.ParentTeamId == parentTeamId.Value);
        }

        query = new TeamSearchSpecification(search, dbContext.Database.IsNpgsql()).Apply(query);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Team> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<PagedResult<Team>> GetSubTeamsAsync(Guid teamId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = dbContext.Teams.AsNoTracking().Where(t => t.ParentTeamId == teamId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Team> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<PagedResult<Employee>> GetEmployeesAsync(Guid teamId, int page, int pageSize, CancellationToken ct = default)
    {
        var teamEmployeeIds = dbContext.EmployeeTeams
            .Where(ct2 => ct2.TeamId == teamId)
            .Select(ct2 => ct2.EmployeeId);

        var query = dbContext.Employees
            .AsNoTracking()
            .Where(c => teamEmployeeIds.Contains(c.Id));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Employee> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<List<Employee>> GetEmployeeLookupAsync(Guid teamId, CancellationToken ct = default)
    {
        return await dbContext.EmployeeTeams
            .AsNoTracking()
            .Where(ct2 => ct2.TeamId == teamId)
            .Include(ct2 => ct2.Employee)
            .Select(ct2 => ct2.Employee)
            .OrderBy(c => c.FullName)
            .ToListAsync(ct);
    }

    public async Task<List<OrganizationEmployeeMember>> GetEligibleEmployeesForAssignmentAsync(
        Guid teamId, Guid organizationId, string? search, int limit, CancellationToken ct = default)
    {
        var currentEmployeeIds = await dbContext.EmployeeTeams
            .Where(et => et.TeamId == teamId)
            .Select(et => et.EmployeeId)
            .ToListAsync(ct);

        var query = dbContext.OrganizationEmployeeMembers
            .AsNoTracking()
            .Include(m => m.Employee)
            .Where(m => m.OrganizationId == organizationId)
            .Where(m => !currentEmployeeIds.Contains(m.EmployeeId));

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = new EmployeeSearchSpecification(search, dbContext.Database.IsNpgsql()).Apply(query);
        }

        return await query
            .OrderBy(m => m.Employee.FullName)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Teams.AnyAsync(t => t.Id == id, ct);

    public async Task<bool> HasSubTeamsAsync(Guid teamId, CancellationToken ct = default)
        => await dbContext.Teams.AnyAsync(t => t.ParentTeamId == teamId, ct);

    public async Task<bool> HasMissionsAsync(Guid teamId, CancellationToken ct = default)
    {
        var employeeIds = await dbContext.EmployeeTeams
            .AsNoTracking()
            .Where(employeeTeam => employeeTeam.TeamId == teamId)
            .Select(employeeTeam => employeeTeam.EmployeeId)
            .ToListAsync(ct);

        if (employeeIds.Count == 0)
        {
            return false;
        }

        return await dbContext.Missions
            .AsNoTracking()
            .AnyAsync(mission => mission.EmployeeId.HasValue && employeeIds.Contains(mission.EmployeeId.Value), ct);
    }

    public async Task BulkUpdateStatusAsync(IEnumerable<Guid> ids, TeamStatus status, CancellationToken ct = default)
    {
        var idList = ids.ToList();
        await dbContext.Teams
            .Where(t => idList.Contains(t.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.Status, status)
                .SetProperty(t => t.UpdatedAt, DateTime.UtcNow),
            ct);
    }

    public async Task BulkDeleteAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idList = ids.ToList();
        await dbContext.Teams
            .Where(t => idList.Contains(t.Id))
            .ExecuteDeleteAsync(ct);
    }

    public async Task AddAsync(Team entity, CancellationToken ct = default)
        => await dbContext.Teams.AddAsync(entity, ct);

    public async Task RemoveAsync(Team entity, CancellationToken ct = default)
        => await Task.FromResult(dbContext.Teams.Remove(entity));

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await dbContext.SaveChangesAsync(ct);
}
