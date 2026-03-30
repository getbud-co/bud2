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

        var employee = await dbContext.Employees
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                currentEmployee => currentEmployee.Id == tenantProvider.EmployeeId.Value,
                cancellationToken);

        if (employee is null)
        {
            return Result.Forbidden(employeeNotIdentifiedMessage);
        }

        return employee.Role == EmployeeRole.Leader && employee.OrganizationId == organizationId
            ? Result.Success()
            : Result.Forbidden(forbiddenMessage);
    }
}
