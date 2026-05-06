using Bud.Infrastructure.Persistence;
using Bud.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure.Features.Indicators;

public sealed class IndicatorRepository(ApplicationDbContext dbContext) : IIndicatorRepository
{
    public async Task<Indicator?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Indicators
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<Indicator?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Indicators.FindAsync([id], ct);

    public async Task<PagedResult<Indicator>> GetAllAsync(
        Guid? missionId, string? search, int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = PaginationNormalizer.Normalize(page, pageSize);

        var query = dbContext.Indicators.AsNoTracking();

        if (missionId.HasValue)
        {
            query = query.Where(i => i.MissionId == missionId.Value);
        }

        query = new IndicatorSearchSpecification(search, dbContext.Database.IsNpgsql()).Apply(query);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(i => i.Title)
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

    public async Task<Mission?> GetMissionByIdAsync(Guid missionId, CancellationToken ct = default)
        => await dbContext.Missions
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == missionId, ct);

    public async Task<Checkin?> GetCheckinByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Checkins
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Checkin?> GetCheckinByIdForUpdateAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Checkins.FindAsync([id], ct);

    public async Task<PagedResult<Checkin>> GetCheckinsAsync(
        Guid? indicatorId, Guid? missionId, int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = PaginationNormalizer.Normalize(page, pageSize);

        var query = dbContext.Checkins.AsNoTracking();

        if (indicatorId.HasValue)
        {
            query = query.Where(c => c.IndicatorId == indicatorId.Value);
        }

        if (missionId.HasValue)
        {
            query = query.Where(c => c.Indicator.MissionId == missionId.Value);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        if (items.Count > 0)
        {
            var employeeIds = items.Select(c => c.EmployeeId).Distinct().ToList();
            var employees = await dbContext.Employees
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => employeeIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, ct);

            foreach (var item in items)
            {
                if (employees.TryGetValue(item.EmployeeId, out var employee))
                {
                    item.Employee = employee;
                }
            }
        }

        return new PagedResult<Checkin>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Indicator?> GetIndicatorWithMissionAsync(Guid indicatorId, CancellationToken ct = default)
        => await dbContext.Indicators
            .Include(i => i.Mission)
            .FirstOrDefaultAsync(i => i.Id == indicatorId, ct);

    public async Task AddCheckinAsync(Checkin entity, CancellationToken ct = default)
        => await dbContext.Checkins.AddAsync(entity, ct);

    public Task RemoveCheckinAsync(Checkin entity, CancellationToken ct = default)
    {
        dbContext.Checkins.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task AddAsync(Indicator entity, CancellationToken ct = default)
        => await dbContext.Indicators.AddAsync(entity, ct);

    public Task RemoveAsync(Indicator entity, CancellationToken ct = default)
    {
        dbContext.Indicators.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await dbContext.SaveChangesAsync(ct);
}
