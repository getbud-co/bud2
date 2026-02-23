using Bud.Server.Domain.Model;

namespace Bud.Server.Domain.Repositories;

public interface ITemplateRepository
{
    Task<Template?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Template?> GetByIdWithChildrenAsync(Guid id, CancellationToken ct = default);
    Task<Template?> GetByIdReadOnlyAsync(Guid id, CancellationToken ct = default);
    Task<Bud.Shared.Contracts.Common.PagedResult<Template>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Template entity, CancellationToken ct = default);
    Task RemoveAsync(Template entity, CancellationToken ct = default);
    Task RemoveObjectivesAndMetricsAsync(IEnumerable<TemplateObjective> objectives, IEnumerable<TemplateMetric> metrics, CancellationToken ct = default);
    Task AddObjectivesAndMetricsAsync(IEnumerable<TemplateObjective> objectives, IEnumerable<TemplateMetric> metrics, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
