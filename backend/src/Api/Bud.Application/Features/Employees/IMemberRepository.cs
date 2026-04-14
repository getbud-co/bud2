
namespace Bud.Application.Features.Employees;

public interface IMemberRepository
{
    Task<OrganizationEmployeeMember?> GetByIdAsync(Guid employeeId, CancellationToken ct = default);
    Task<OrganizationEmployeeMember?> GetByIdWithEmployeeTeamsAsync(Guid employeeId, CancellationToken ct = default);
    Task<PagedResult<OrganizationEmployeeMember>> GetAllAsync(Guid? teamId, string? search, int page, int pageSize, CancellationToken ct = default);
    Task<List<OrganizationEmployeeMember>> GetLeadersAsync(CancellationToken ct = default);
    Task<List<OrganizationEmployeeMember>> GetSubordinatesAsync(Guid employeeId, int maxDepth, CancellationToken ct = default);
    Task<List<Team>> GetTeamsAsync(Guid employeeId, CancellationToken ct = default);
    Task<List<Team>> GetEligibleTeamsForAssignmentAsync(Guid employeeId, Guid organizationId, string? search, int limit, CancellationToken ct = default);
    Task<List<OrganizationEmployeeMember>> GetLookupAsync(string? search, int limit, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid employeeId, CancellationToken ct = default);
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId, CancellationToken ct = default);
    Task<bool> HasSubordinatesAsync(Guid employeeId, CancellationToken ct = default);
    Task<bool> HasMissionsAsync(Guid employeeId, CancellationToken ct = default);
    Task<int> CountTeamsByIdsAndOrganizationAsync(List<Guid> teamIds, Guid organizationId, CancellationToken ct = default);
    Task<int> CountByIdsAndOrganizationAsync(List<Guid> ids, Guid organizationId, CancellationToken ct = default);
    Task<bool> IsValidLeaderAsync(Guid leaderId, Guid? requiredOrganizationId, CancellationToken ct = default);
    Task AddAsync(Employee employee, OrganizationEmployeeMember member, CancellationToken ct = default);
    Task RemoveAsync(OrganizationEmployeeMember member, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
