using Bud.Application.Common;
using Bud.Application.Features.Me;
using Bud.Application.Ports;

namespace Bud.Infrastructure.Features.Me;

public sealed class DashboardAuthorizationService(ITenantProvider tenantProvider)
    : IReadAuthorizationRule<DashboardResource>
{
    public Task<Result> EvaluateAsync(DashboardResource resource, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            tenantProvider.EmployeeId.HasValue
                ? Result.Success()
                : Result.Forbidden("Colaborador não identificado."));
    }
}
