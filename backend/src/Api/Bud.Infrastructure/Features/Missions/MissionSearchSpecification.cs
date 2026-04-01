using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure.Features.Missions;

public sealed class MissionSearchSpecification(string? search, bool isNpgsql) : IQuerySpecification<Mission>
{
    public IQueryable<Mission> Apply(IQueryable<Mission> query)
    {
        return QuerySearchHelper.ApplyCaseInsensitiveSearch(
            query,
            search,
            isNpgsql,
            (q, pattern) => q.Where(g => EF.Functions.ILike(g.Name, pattern)),
            (q, term) => q.Where(g => g.Name.Contains(term, StringComparison.OrdinalIgnoreCase)));
    }
}
