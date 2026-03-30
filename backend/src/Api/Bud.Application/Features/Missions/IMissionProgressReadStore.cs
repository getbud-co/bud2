using Bud.Application.Common;

namespace Bud.Application.Features.Missions;

public interface IMissionProgressReadStore
{
    Task<Result<List<MissionProgressSnapshot>>> GetProgressAsync(List<Guid> missionIds, CancellationToken ct = default);
}
