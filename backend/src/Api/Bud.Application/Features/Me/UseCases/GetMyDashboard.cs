using Bud.Application.Common;
using Bud.Application.Ports;

namespace Bud.Application.Features.Me.UseCases;

public sealed class GetMyDashboard(
    IMyDashboardReadStore dashboardReadStore,
    ITenantProvider tenantProvider)
{
    public async Task<Result<MyDashboardResponse>> ExecuteAsync(
        Guid? teamId = null,
        CancellationToken cancellationToken = default)
    {
        if (!tenantProvider.EmployeeId.HasValue)
        {
            return Result<MyDashboardResponse>.Forbidden(UserErrorMessages.EmployeeNotIdentified);
        }

        var snapshot = await dashboardReadStore.GetMyDashboardAsync(
            tenantProvider.EmployeeId.Value,
            teamId,
            cancellationToken);

        if (snapshot is null)
        {
            return Result<MyDashboardResponse>.NotFound(UserErrorMessages.EmployeeNotFound);
        }

        return Result<MyDashboardResponse>.Success(snapshot.ToResponse());
    }
}
