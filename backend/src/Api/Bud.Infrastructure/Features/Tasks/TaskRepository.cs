using Bud.Application.Common;
using Bud.Infrastructure.Persistence;
using Bud.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure.Features.Tasks;

public sealed class TaskRepository(ApplicationDbContext dbContext) : ITaskRepository
{
    public async Task<MissionTask?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.MissionTasks.FindAsync([id], ct);

    public async Task<Mission?> GetMissionByIdAsync(Guid missionId, CancellationToken ct = default)
        => await dbContext.Missions
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == missionId, ct);

    public async Task<PagedResult<MissionTask>> GetByMissionIdAsync(Guid missionId, int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = PaginationNormalizer.Normalize(page, pageSize);

        var query = dbContext.MissionTasks
            .AsNoTracking()
            .Where(t => t.MissionId == missionId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(t => t.IsDone)
            .ThenBy(t => t.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<MissionTask>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<MissionTask>> GetActiveTasksForMissionsAsync(List<Guid> missionIds, CancellationToken ct = default)
        => await dbContext.MissionTasks
            .AsNoTracking()
            .Where(t => missionIds.Contains(t.MissionId) && !t.IsDone)
            .OrderBy(t => t.CreatedAt)
            .ThenBy(t => t.Title)
            .ToListAsync(ct);

    public async Task AddAsync(MissionTask entity, CancellationToken ct = default)
        => await dbContext.MissionTasks.AddAsync(entity, ct);

    public Task RemoveAsync(MissionTask entity, CancellationToken ct = default)
    {
        dbContext.MissionTasks.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await dbContext.SaveChangesAsync(ct);
}
