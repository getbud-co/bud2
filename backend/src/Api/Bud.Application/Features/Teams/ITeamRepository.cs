
namespace Bud.Application.Features.Teams;

public interface ITeamRepository
{
    Task<Team?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Team?> GetByIdWithEmployeeTeamsAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Team>> GetAllAsync(Guid? parentTeamId, string? search, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<Team>> GetSubTeamsAsync(Guid teamId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<Employee>> GetEmployeesAsync(Guid teamId, int page, int pageSize, CancellationToken ct = default);
    Task<List<Employee>> GetEmployeeLookupAsync(Guid teamId, CancellationToken ct = default);
    Task<List<Employee>> GetEligibleEmployeesForAssignmentAsync(Guid teamId, string? search, int limit, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> HasSubTeamsAsync(Guid teamId, CancellationToken ct = default);
    Task<bool> HasMissionsAsync(Guid teamId, CancellationToken ct = default);
    Task BulkUpdateStatusAsync(IEnumerable<Guid> ids, TeamStatus status, CancellationToken ct = default);
    Task BulkDeleteAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task AddAsync(Team entity, CancellationToken ct = default);
    Task RemoveAsync(Team entity, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
