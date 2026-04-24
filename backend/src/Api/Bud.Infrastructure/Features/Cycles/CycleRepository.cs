using Bud.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure.Features.Cycles;

public sealed class CycleRepository(ApplicationDbContext dbContext) : ICycleRepository
{
    public async Task<Cycle?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Cycles.FindAsync([id], ct);

    public async Task<List<Cycle>> GetAllAsync(Guid organizationId, CancellationToken ct = default)
        => await dbContext.Cycles
            .AsNoTracking()
            .Where(c => c.OrganizationId == organizationId)
            .OrderBy(c => c.StartDate)
            .ToListAsync(ct);

    public async Task AddAsync(Cycle entity, CancellationToken ct = default)
        => await dbContext.Cycles.AddAsync(entity, ct);

    public Task RemoveAsync(Cycle entity, CancellationToken ct = default)
    {
        dbContext.Cycles.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await dbContext.SaveChangesAsync(ct);
}
