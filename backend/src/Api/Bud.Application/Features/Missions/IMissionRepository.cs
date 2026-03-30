
namespace Bud.Application.Features.Missions;

public interface IMissionRepository
{
    Task<Mission?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Mission?> GetByIdReadOnlyAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Mission>> GetAllAsync(
        MissionFilter? filter, Guid? employeeId, string? search,
        int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<Mission>> GetChildrenAsync(Guid parentId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<Indicator>> GetIndicatorsAsync(Guid missionId, int page, int pageSize, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Mission entity, CancellationToken ct = default);
    Task RemoveAsync(Mission entity, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
