
namespace Bud.Application.Features.Indicators;

public interface IIndicatorRepository
{
    Task<Indicator?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Indicator?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Indicator>> GetAllAsync(Guid? missionId, string? search, int page, int pageSize, CancellationToken ct = default);
    Task<Mission?> GetMissionByIdAsync(Guid missionId, CancellationToken ct = default);
    Task<Checkin?> GetCheckinByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Checkin>> GetCheckinsAsync(Guid? indicatorId, Guid? missionId, int page, int pageSize, CancellationToken ct = default);
    Task<Indicator?> GetIndicatorWithMissionAsync(Guid indicatorId, CancellationToken ct = default);
    Task AddCheckinAsync(Checkin entity, CancellationToken ct = default);
    Task RemoveCheckinAsync(Checkin entity, CancellationToken ct = default);
    Task AddAsync(Indicator entity, CancellationToken ct = default);
    Task RemoveAsync(Indicator entity, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
