
namespace Bud.Application.Features.Organizations;

public interface IOrganizationRepository
{
    Task<Organization?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Organization>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<Collaborator>> GetCollaboratorsAsync(Guid organizationId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<Cycle>> GetCyclesAsync(Guid organizationId, int page, int pageSize, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> HasCollaboratorsAsync(Guid organizationId, CancellationToken ct = default);
    Task AddAsync(Organization entity, CancellationToken ct = default);
    Task RemoveAsync(Organization entity, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
