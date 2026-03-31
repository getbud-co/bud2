
using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure.Features.Teams;

public sealed class TeamSearchSpecification(string? search, bool isNpgsql) : IQuerySpecification<Team>
{
    public IQueryable<Team> Apply(IQueryable<Team> query)
    {
        return QuerySearchHelper.ApplyCaseInsensitiveSearch(
            query,
            search,
            isNpgsql,
            ApplyNpgsql,
            ApplyInMemory);
    }

    private IQueryable<Team> ApplyNpgsql(IQueryable<Team> query, string pattern)
        => query.Where(t => EF.Functions.ILike(t.Name, pattern));

    private IQueryable<Team> ApplyInMemory(IQueryable<Team> query, string term)
        => query.Where(t => t.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
}
