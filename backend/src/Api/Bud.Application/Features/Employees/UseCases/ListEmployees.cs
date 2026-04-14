using Bud.Application.Common;
using Bud.Shared.Contracts;

namespace Bud.Application.Features.Employees.UseCases;

public sealed class ListEmployees(IMemberRepository employeeRepository)
{
    public async Task<Result<PagedResult<OrganizationEmployeeMember>>> ExecuteAsync(
        Guid? teamId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        (page, pageSize) = PaginationNormalizer.Normalize(page, pageSize);
        var result = await employeeRepository.GetAllAsync(teamId, search, page, pageSize, cancellationToken);
        return Result<PagedResult<OrganizationEmployeeMember>>.Success(result.MapPaged(x => x));
    }
}
