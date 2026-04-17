namespace Bud.Application.Features.Employees;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Employee>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct = default);
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId, CancellationToken ct = default);
    Task AddAsync(Employee entity, CancellationToken ct = default);
    Task RemoveAsync(Employee entity, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
