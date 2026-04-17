using Bud.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure.Features.Employees;

public sealed class EmployeeRepository(ApplicationDbContext dbContext) : IEmployeeRepository
{
    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => dbContext.Employees.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<PagedResult<Employee>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var query = ApplySearch(dbContext.Employees.AsNoTracking(), search);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => EF.Property<string>(c, nameof(Employee.FullName)))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Employee>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> IsEmailUniqueAsync(EmailAddress email, Guid? excludeId, CancellationToken ct = default)
    {
        var query = dbContext.Employees.AsQueryable();

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return !await query.AnyAsync(c => EF.Property<string>(c, nameof(Employee.Email)) == email.Value, ct);
    }

    public Task AddAsync(Employee entity, CancellationToken ct = default)
        => dbContext.Employees.AddAsync(entity, ct).AsTask();

    public Task RemoveAsync(Employee entity, CancellationToken ct = default)
    {
        dbContext.Employees.Remove(entity);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => dbContext.SaveChangesAsync(ct);

    private static IQueryable<Employee> ApplySearch(IQueryable<Employee> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var normalized = $"%{search.Trim()}%";
        return query.Where(c =>
            EF.Functions.ILike(EF.Property<string>(c, nameof(Employee.FullName)), normalized) ||
            EF.Functions.ILike(EF.Property<string>(c, nameof(Employee.Email)), normalized));
    }
}
