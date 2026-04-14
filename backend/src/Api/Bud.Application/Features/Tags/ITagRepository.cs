namespace Bud.Application.Features.Tags;

public sealed record TagWithCount(Tag Tag, int LinkedItems);

public interface ITagRepository
{
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Tag?> GetByIdReadOnlyAsync(Guid id, CancellationToken ct = default);
    Task<List<Tag>> GetAllAsync(CancellationToken ct = default);
    Task<List<TagWithCount>> GetAllWithCountsAsync(CancellationToken ct = default);
    Task<int> GetLinkedItemsCountAsync(Guid tagId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> IsNameUniqueAsync(string name, Guid organizationId, Guid? excludeId, CancellationToken ct = default);
    Task<MissionTag?> GetMissionTagAsync(Guid missionId, Guid tagId, CancellationToken ct = default);
    Task AddAsync(Tag entity, CancellationToken ct = default);
    Task RemoveAsync(Tag entity, CancellationToken ct = default);
    Task AddMissionTagAsync(MissionTag missionTag, CancellationToken ct = default);
    Task RemoveMissionTagAsync(MissionTag missionTag, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
