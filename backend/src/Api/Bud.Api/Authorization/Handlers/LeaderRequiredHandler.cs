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
        if (tenantProvider.IsGlobalAdmin || context.User.IsInRole(nameof(EmployeeRole.Leader)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
