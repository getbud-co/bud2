using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;
using Bud.Shared.Contracts;

namespace Bud.Application.Features.Teams.UseCases;

public sealed class ListTeams(ITeamRepository teamRepository)
{
    public async Task<Result<PagedResult<Team>>> ExecuteAsync(
        Guid? parentTeamId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        (page, pageSize) = PaginationNormalizer.Normalize(page, pageSize);
        var result = await teamRepository.GetAllAsync(parentTeamId, search, page, pageSize, cancellationToken);
        return Result<PagedResult<Team>>.Success(result.MapPaged(x => x));
    }
}
