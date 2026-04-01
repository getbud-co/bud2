
namespace Bud.Application.Features.Tasks;

public interface ITaskRepository
{
    Task<MissionTask?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Mission?> GetMissionByIdAsync(Guid missionId, CancellationToken ct = default);
    Task<PagedResult<MissionTask>> GetByMissionIdAsync(Guid missionId, int page, int pageSize, CancellationToken ct = default);
    Task<List<MissionTask>> GetActiveTasksForMissionsAsync(List<Guid> missionIds, CancellationToken ct = default);
    Task AddAsync(MissionTask entity, CancellationToken ct = default);
    Task RemoveAsync(MissionTask entity, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
