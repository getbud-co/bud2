using Bud.Application.Common;
using Bud.Application.Ports;
using Bud.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure.Authorization;

public static class LeaderScopedAuthorization
{
    public static async Task<Result> RequireLeaderInOrganizationAsync(
        ApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        Guid organizationId,
        string employeeNotIdentifiedMessage,
        string forbiddenMessage,
        CancellationToken cancellationToken = default)
    {
        if (tenantProvider.IsGlobalAdmin)
        {
            return Result.Success();
        }

        if (tenantProvider.EmployeeId is null)
        {
            return Result.Forbidden(employeeNotIdentifiedMessage);
        }

        var member = await dbContext.OrganizationEmployeeMembers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                m => m.EmployeeId == tenantProvider.EmployeeId.Value,
                cancellationToken);

        if (member is null)
        {
            return Result.Forbidden(employeeNotIdentifiedMessage);
        }

        return member.Role == EmployeeRole.Leader && member.OrganizationId == organizationId
            ? Result.Success()
            : Result.Forbidden(forbiddenMessage);
    }
}
