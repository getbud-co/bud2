using Bud.Api.Authorization.Requirements;
using Bud.Application.Ports;
using Bud.Infrastructure.Persistence;
using Bud.Shared.Kernel.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Bud.Api.Authorization.Handlers;

public sealed class MissionOwnerOrTeamLeaderHandler(
    ApplicationDbContext dbContext,
    ITenantProvider tenantProvider,
    IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<MissionOwnerOrTeamLeaderRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MissionOwnerOrTeamLeaderRequirement requirement)
    {
        if (tenantProvider.IsGlobalAdmin)
        {
            context.Succeed(requirement);
            return;
        }

        if (!tenantProvider.EmployeeId.HasValue || !tenantProvider.TenantId.HasValue)
            return;

        var employee = await dbContext.Employees
            .Include(e => e.Memberships)
            .FirstOrDefaultAsync(e => e.Id == tenantProvider.EmployeeId.Value);

        if (employee is null)
            return;

        if (employee.HasMinimumRoleIn(tenantProvider.TenantId.Value, EmployeeRole.TeamLeader))
        {
            context.Succeed(requirement);
            return;
        }

        var routeValues = httpContextAccessor.HttpContext?.GetRouteData().Values;
        if (routeValues is null ||
            !routeValues.TryGetValue("id", out var idValue) ||
            !Guid.TryParse(idValue?.ToString(), out var missionId))
            return;

        var missionOwnerId = await dbContext.Missions
            .Where(m => m.Id == missionId)
            .Select(m => (Guid?)m.EmployeeId)
            .FirstOrDefaultAsync();

        if (missionOwnerId.HasValue && missionOwnerId == tenantProvider.EmployeeId)
        {
            context.Succeed(requirement);
        }
    }
}
