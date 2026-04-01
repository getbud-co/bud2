using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;
using Bud.Shared.Contracts;

namespace Bud.Application.Features.Teams.UseCases;

public sealed class ListTeamEmployees(ITeamRepository teamRepository)
{
    public async Task<Result<PagedResult<Employee>>> ExecuteAsync(
        Guid id,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        (page, pageSize) = PaginationNormalizer.Normalize(page, pageSize);

        if (!await teamRepository.ExistsAsync(id, cancellationToken))
        {
            return Result<PagedResult<Employee>>.NotFound(UserErrorMessages.TeamNotFound);
        }

        var result = await teamRepository.GetEmployeesAsync(id, page, pageSize, cancellationToken);
        return Result<PagedResult<Employee>>.Success(result.MapPaged(x => x));
    }
}

