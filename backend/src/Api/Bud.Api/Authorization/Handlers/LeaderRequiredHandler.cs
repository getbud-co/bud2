using Bud.Api.Authorization.Requirements;
using Bud.Application.Ports;
using Microsoft.AspNetCore.Authorization;

namespace Bud.Api.Authorization.Handlers;

public sealed class LeaderRequiredHandler(ITenantProvider tenantProvider)
    : AuthorizationHandler<LeaderRequiredRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        LeaderRequiredRequirement requirement)
    {
        if (tenantProvider.IsGlobalAdmin)
        {
            context.Succeed(requirement);
            return;
        }

        if (tenantProvider.EmployeeId is null || !tenantProvider.TenantId.HasValue)
        {
            return;
        }

        var employee = await dbContext.Employees
            .Include(e => e.Memberships)
            .FirstOrDefaultAsync(e => e.Id == tenantProvider.EmployeeId.Value);

        if (employee?.HasMinimumRoleIn(tenantProvider.TenantId.Value, EmployeeRole.TeamLeader) == true)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
