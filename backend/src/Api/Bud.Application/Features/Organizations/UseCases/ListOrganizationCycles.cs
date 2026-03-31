using Bud.Application.Common;
using Bud.Shared.Contracts;

namespace Bud.Application.Features.Organizations.UseCases;

public sealed class ListOrganizationCycles(IOrganizationRepository organizationRepository)
{
    public async Task<Result<PagedResult<Cycle>>> ExecuteAsync(
        Guid id,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        (page, pageSize) = PaginationNormalizer.Normalize(page, pageSize);

        if (!await organizationRepository.ExistsAsync(id, cancellationToken))
        {
            return Result<PagedResult<Cycle>>.NotFound(UserErrorMessages.OrganizationNotFound);
        }

        var result = await organizationRepository.GetCyclesAsync(id, page, pageSize, cancellationToken);
        return Result<PagedResult<Cycle>>.Success(result.MapPaged(x => x));
    }
}
