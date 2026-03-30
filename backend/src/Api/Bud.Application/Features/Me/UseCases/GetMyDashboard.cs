using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;
using Bud.Shared.Contracts;

namespace Bud.Application.Features.Me.UseCases;

public sealed class GetMyDashboard(
    IMyDashboardReadStore dashboardReadStore,
    ITenantProvider tenantProvider,
    IApplicationAuthorizationGateway authorizationGateway)
{
    public async Task<Result<MyDashboardResponse>> ExecuteAsync(
        ClaimsPrincipal user,
        Guid? teamId = null,
        CancellationToken cancellationToken = default)
    {
        if (!await authorizationGateway.CanReadAsync(user, DashboardResource.Instance, cancellationToken))
        {
            return Result<MyDashboardResponse>.Forbidden(UserErrorMessages.EmployeeNotIdentified);
        }

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
