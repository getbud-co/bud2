namespace Bud.Application.Features.Me;

public interface IMyDashboardReadStore
{
    Task<DashboardSnapshot?> GetMyDashboardAsync(
        Guid employeeId,
        Guid? teamId,
        CancellationToken ct = default);
}
