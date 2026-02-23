using Bud.Server.Application.ReadModels;

namespace Bud.Server.Application.Ports;

public interface IMyDashboardReadStore
{
    Task<DashboardSnapshot?> GetMyDashboardAsync(
        Guid collaboratorId,
        Guid? teamId,
        CancellationToken ct = default);
}
