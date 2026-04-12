using Bud.Api.Authorization.Requirements;
using Bud.Application.Ports;
using Bud.Infrastructure.Persistence;
using Bud.Shared.Kernel.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Bud.Api.Authorization.Handlers;

public sealed class LeaderRequiredHandler(
    ApplicationDbContext dbContext,
    ITenantProvider tenantProvider)
    : AuthorizationHandler<LeaderRequiredRequirement>
{
    protected override async Task HandleRequirementAsync(
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

        var member = await dbContext.OrganizationEmployeeMembers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m =>
                m.EmployeeId == tenantProvider.EmployeeId.Value &&
                m.OrganizationId == tenantProvider.TenantId.Value);

        if (member?.Role == EmployeeRole.TeamLeader)
        {
            context.Succeed(requirement);
        }
    }
}
