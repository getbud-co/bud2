using Bud.Application.Features.Tags;
using Bud.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure.Features.Tags;

public sealed class TagRepository(ApplicationDbContext dbContext) : ITagRepository
{
    public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Tags.FindAsync([id], ct);

    public async Task<Tag?> GetByIdReadOnlyAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Tags
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<List<Tag>> GetAllAsync(CancellationToken ct = default)
        => await dbContext.Tags
            .AsNoTracking()
            .Where(t => t.DeletedAt == null)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);

    public async Task<List<TagWithCount>> GetAllWithCountsAsync(CancellationToken ct = default)
        => await dbContext.Tags
            .AsNoTracking()
            .Where(t => t.DeletedAt == null)
            .OrderBy(t => t.Name)
            .Select(t => new TagWithCount(t, t.MissionTags.Count))
            .ToListAsync(ct);

    public async Task<int> GetLinkedItemsCountAsync(Guid tagId, CancellationToken ct = default)
        => await dbContext.MissionTags
            .CountAsync(mt => mt.TagId == tagId, ct);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Tags.AnyAsync(t => t.Id == id && t.DeletedAt == null, ct);

    public async Task<bool> IsNameUniqueAsync(string name, Guid organizationId, Guid? excludeId, CancellationToken ct = default)
        => !await dbContext.Tags.AnyAsync(
            t => t.OrganizationId == organizationId
                 && t.Name == name
                 && t.DeletedAt == null
                 && (excludeId == null || t.Id != excludeId),
            ct);

    public async Task<MissionTag?> GetMissionTagAsync(Guid missionId, Guid tagId, CancellationToken ct = default)
        => await dbContext.MissionTags
            .FirstOrDefaultAsync(mt => mt.MissionId == missionId && mt.TagId == tagId, ct);

    public async Task AddAsync(Tag entity, CancellationToken ct = default)
        => await dbContext.Tags.AddAsync(entity, ct);

    public Task RemoveAsync(Tag entity, CancellationToken ct = default)
    {
        entity.DeletedAt = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    public async Task AddMissionTagAsync(MissionTag missionTag, CancellationToken ct = default)
        => await dbContext.MissionTags.AddAsync(missionTag, ct);

    public Task RemoveMissionTagAsync(MissionTag missionTag, CancellationToken ct = default)
    {
        dbContext.MissionTags.Remove(missionTag);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await dbContext.SaveChangesAsync(ct);
}
