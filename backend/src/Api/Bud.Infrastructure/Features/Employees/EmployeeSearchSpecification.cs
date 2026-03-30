
using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure.Features.Employees;

public sealed class EmployeeSearchSpecification(string? search, bool isNpgsql) : IQuerySpecification<Employee>
{
    public IQueryable<Employee> Apply(IQueryable<Employee> query)
    {
        return QuerySearchHelper.ApplyCaseInsensitiveSearch(
            query,
            search,
            isNpgsql,
            (q, pattern) => q.Where(c => EF.Functions.ILike(c.FullName, pattern) || EF.Functions.ILike(c.Email, pattern)),
            (q, term) => q.Where(c =>
                c.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                c.Email.Contains(term, StringComparison.OrdinalIgnoreCase)));
    }
}
