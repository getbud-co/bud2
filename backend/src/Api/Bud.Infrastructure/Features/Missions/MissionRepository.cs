using Bud.Infrastructure.Persistence;
using Bud.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure.Features.Missions;

public sealed class MissionRepository(ApplicationDbContext dbContext) : IMissionRepository
{
    public async Task<Mission?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Missions.FindAsync([id], ct);

    public async Task<Mission?> GetByIdReadOnlyAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Missions
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id, ct);

    public async Task<PagedResult<Mission>> GetAllAsync(
        MissionFilter? filter, Guid? employeeId, string? search,
        int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = PaginationNormalizer.Normalize(page, pageSize);

        var query = dbContext.Missions.AsNoTracking();

        query = new MissionSearchSpecification(search, dbContext.Database.IsNpgsql()).Apply(query);

        if (filter is MissionFilter.Mine && employeeId.HasValue)
        {
            var rootIds = await FindRootMissionIdsForEmployeesAsync(
                [employeeId.Value], ct);

            query = query.Where(g => g.ParentId == null && rootIds.Contains(g.Id));
        }
        else if (filter is MissionFilter.MyTeam && employeeId.HasValue)
        {
            var teamEmployeeIds = await GetTeamEmployeeIdsAsync(
                employeeId.Value, ct);

            var rootIds = await FindRootMissionIdsForEmployeesAsync(
                teamEmployeeIds, ct);

            query = query.Where(g => g.ParentId == null && rootIds.Contains(g.Id));
        }
        else
        {
            // MissionFilter.All or no filter — just root missions with tenant filter
            query = query.Where(g => g.ParentId == null);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(g => g.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Mission>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<Mission>> GetChildrenAsync(Guid parentId, int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = PaginationNormalizer.Normalize(page, pageSize);

        var query = dbContext.Missions
            .AsNoTracking()
            .Where(g => g.ParentId == parentId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(g => g.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Mission>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<Indicator>> GetIndicatorsAsync(Guid missionId, int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = PaginationNormalizer.Normalize(page, pageSize);

        var query = dbContext.Indicators
            .AsNoTracking()
            .Where(i => i.MissionId == missionId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(i => i.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Indicator>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Missions.AnyAsync(g => g.Id == id, ct);

    public async Task AddAsync(Mission entity, CancellationToken ct = default)
        => await dbContext.Missions.AddAsync(entity, ct);

    public Task RemoveAsync(Mission entity, CancellationToken ct = default)
    {
        dbContext.Missions.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await dbContext.SaveChangesAsync(ct);

    /// <summary>
    /// Two-pass: loads all missions (Id, ParentId, EmployeeId), finds which ones
    /// have any of the given employeeIds in their subtree, and walks up to roots.
    /// </summary>
    private async Task<HashSet<Guid>> FindRootMissionIdsForEmployeesAsync(
        HashSet<Guid> employeeIds, CancellationToken ct)
    {
        var allMissions = await dbContext.Missions
            .AsNoTracking()
            .Select(g => new { g.Id, g.ParentId, g.EmployeeId })
            .ToListAsync(ct);

        var parentMap = allMissions.ToDictionary(g => g.Id, g => g.ParentId);

        var rootIds = new HashSet<Guid>();
        foreach (var mission in allMissions)
        {
            if (mission.EmployeeId.HasValue && employeeIds.Contains(mission.EmployeeId.Value))
            {
                // Walk up to root
                var current = mission.Id;
                while (parentMap.TryGetValue(current, out var parentId) && parentId.HasValue)
                {
                    current = parentId.Value;
                }
                rootIds.Add(current);
            }
        }

        return rootIds;
    }

    /// <summary>
    /// Gets all employee IDs from the same teams as the given employee.
    /// </summary>
    private async Task<HashSet<Guid>> GetTeamEmployeeIdsAsync(
        Guid employeeId, CancellationToken ct)
    {
        var teamIds = await dbContext.EmployeeTeams
            .AsNoTracking()
            .Where(ct2 => ct2.EmployeeId == employeeId)
            .Select(ct2 => ct2.TeamId)
            .ToListAsync(ct);

        if (teamIds.Count == 0)
        {
            return [employeeId];
        }

        var memberIds = await dbContext.EmployeeTeams
            .AsNoTracking()
            .Where(ct2 => teamIds.Contains(ct2.TeamId))
            .Select(ct2 => ct2.EmployeeId)
            .ToListAsync(ct);

        var result = new HashSet<Guid>(memberIds);
        result.Add(employeeId);

        return result;
    }
}
