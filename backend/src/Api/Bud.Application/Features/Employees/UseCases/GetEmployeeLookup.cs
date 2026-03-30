using Bud.Application.Common;
using Bud.Shared.Contracts;

namespace Bud.Application.Features.Employees.UseCases;

public sealed class GetEmployeeLookup(IEmployeeRepository employeeRepository)
{
    public async Task<Result<List<EmployeeLookupResponse>>> ExecuteAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var summaries = await employeeRepository.GetLookupAsync(search, 50, cancellationToken);
        return Result<List<EmployeeLookupResponse>>.Success(summaries.Select(c => c.ToResponse()).ToList());
    }
}
