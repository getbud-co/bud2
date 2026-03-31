namespace Bud.Application.Features.Cycles;

public interface ICycleRepository
{
    Task<Cycle?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Cycle>> GetAllAsync(Guid organizationId, CancellationToken ct = default);
    Task AddAsync(Cycle entity, CancellationToken ct = default);
    Task RemoveAsync(Cycle entity, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
