
using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure.Features.Employees;

public sealed class EmployeeSearchSpecification(string? search, bool isNpgsql) : IQuerySpecification<OrganizationEmployeeMember>
{
    public IQueryable<OrganizationEmployeeMember> Apply(IQueryable<OrganizationEmployeeMember> query)
    {
        return QuerySearchHelper.ApplyCaseInsensitiveSearch(
            query,
            search,
            isNpgsql,
            (q, pattern) => q.Where(m => EF.Functions.ILike(m.Employee.FullName, pattern) || EF.Functions.ILike(m.Employee.Email, pattern)),
            (q, term) => q.Where(m =>
                m.Employee.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                m.Employee.Email.Contains(term, StringComparison.OrdinalIgnoreCase)));
    }
}
