
namespace Bud.Application.Features.Organizations;

public interface IOrganizationRepository
{
    Task<Organization?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Organization>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> HasEmployeesAsync(Guid organizationId, CancellationToken ct = default);
    Task AddAsync(Organization entity, CancellationToken ct = default);
    Task RemoveAsync(Organization entity, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
