
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
            (q, pattern) => q.Where(m => EF.Functions.ILike(m.FullName, pattern) || EF.Functions.ILike(m.Email, pattern)),
            (q, term) => q.Where(m =>
                m.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                m.Email.Contains(term, StringComparison.OrdinalIgnoreCase)));
    }
}
