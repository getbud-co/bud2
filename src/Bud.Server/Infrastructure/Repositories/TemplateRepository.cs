using Bud.Server.Infrastructure.Querying;
using Bud.Server.Infrastructure.Persistence;
using Bud.Server.Domain.Model;
using Microsoft.EntityFrameworkCore;

using Bud.Shared.Contracts;

namespace Bud.Server.Infrastructure.Repositories;

public sealed class TemplateRepository(ApplicationDbContext dbContext) : ITemplateRepository
{
    public async Task<Template?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Templates.FindAsync([id], ct);

    public async Task<Template?> GetByIdWithChildrenAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Templates
            .Include(t => t.Objectives)
            .Include(t => t.Metrics)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<Template?> GetByIdReadOnlyAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Templates
            .AsNoTracking()
            .Include(t => t.Objectives.OrderBy(o => o.OrderIndex))
            .Include(t => t.Metrics.OrderBy(m => m.OrderIndex))
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<PagedResult<Template>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var query = dbContext.Templates
            .AsNoTracking()
            .Include(t => t.Objectives.OrderBy(o => o.OrderIndex))
            .Include(t => t.Metrics.OrderBy(m => m.OrderIndex));

        IQueryable<Template> filteredQuery = new TemplateSearchSpecification(search, dbContext.Database.IsNpgsql()).Apply(query);

        var total = await filteredQuery.CountAsync(ct);
        var items = await filteredQuery
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Template>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task AddAsync(Template entity, CancellationToken ct = default)
        => await dbContext.Templates.AddAsync(entity, ct);

    public async Task RemoveAsync(Template entity, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        dbContext.Templates.Remove(entity);
    }

    public async Task RemoveObjectivesAndMetricsAsync(IEnumerable<TemplateObjective> objectives, IEnumerable<TemplateMetric> metrics, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        dbContext.TemplateMetrics.RemoveRange(metrics);
        dbContext.TemplateObjectives.RemoveRange(objectives);
    }

    public async Task AddObjectivesAndMetricsAsync(IEnumerable<TemplateObjective> objectives, IEnumerable<TemplateMetric> metrics, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        dbContext.TemplateObjectives.AddRange(objectives);
        dbContext.TemplateMetrics.AddRange(metrics);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await dbContext.SaveChangesAsync(ct);
}
