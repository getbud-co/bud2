using Bud.Application.Common;
using Bud.Shared.Contracts;

namespace Bud.Application.Features.Employees.UseCases;

public sealed class ListLeaderEmployees(IEmployeeRepository employeeRepository)
{
    public async Task<Result<List<EmployeeLeaderResponse>>> ExecuteAsync(
        Guid? organizationId,
        CancellationToken cancellationToken = default)
    {
        var leaders = await employeeRepository.GetLeadersAsync(organizationId, cancellationToken);
        return Result<List<EmployeeLeaderResponse>>.Success(leaders.Select(m => m.ToLeaderResponse()).ToList());
    }
}
