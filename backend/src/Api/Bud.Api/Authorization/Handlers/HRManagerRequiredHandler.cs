using Bud.Api.Authorization.Requirements;
using Bud.Application.Ports;
using Bud.Infrastructure.Persistence;
using Bud.Shared.Kernel.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Bud.Api.Authorization.Handlers;

public sealed class HRManagerRequiredHandler(
    ApplicationDbContext dbContext,
    ITenantProvider tenantProvider)
    : AuthorizationHandler<HRManagerRequiredRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        HRManagerRequiredRequirement requirement)
    {
        if (tenantProvider.IsGlobalAdmin)
        {
            context.Succeed(requirement);
            return;
        }

        if (!tenantProvider.EmployeeId.HasValue || !tenantProvider.TenantId.HasValue)
        {
            return;
        }

        var employee = await dbContext.Employees
            .Include(e => e.Memberships)
            .FirstOrDefaultAsync(e => e.Id == tenantProvider.EmployeeId.Value);

        if (employee?.HasMinimumRoleIn(tenantProvider.TenantId.Value, EmployeeRole.HRManager) == true)
        {
            context.Succeed(requirement);
        }
    }
}
