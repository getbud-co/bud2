using Bud.Application.Common;
using Bud.Shared.Contracts;

namespace Bud.Application.Features.Organizations.UseCases;

public sealed class ListOrganizationEmployees(IOrganizationRepository organizationRepository)
{
    public async Task<Result<PagedResult<Employee>>> ExecuteAsync(
        Guid id,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        (page, pageSize) = PaginationNormalizer.Normalize(page, pageSize);

        if (!await organizationRepository.ExistsAsync(id, cancellationToken))
        {
            return Result<PagedResult<Employee>>.NotFound(UserErrorMessages.OrganizationNotFound);
        }

        var result = await organizationRepository.GetEmployeesAsync(id, page, pageSize, cancellationToken);
        return Result<PagedResult<Employee>>.Success(result.MapPaged(x => x));
    }
}
