using Bud.Application.Common;
using Bud.Shared.Contracts;

namespace Bud.Application.Features.Employees.UseCases;

public sealed class ListLeaderEmployees(IMemberRepository employeeRepository)
{
    public async Task<Result<List<EmployeeLeaderResponse>>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var leaders = await employeeRepository.GetLeadersAsync(cancellationToken);
        return Result<List<EmployeeLeaderResponse>>.Success(leaders.Select(m => m.ToLeaderResponse()).ToList());
    }
}
