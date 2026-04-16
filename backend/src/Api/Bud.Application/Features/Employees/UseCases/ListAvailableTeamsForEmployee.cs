using Bud.Application.Common;
using Bud.Application.Ports;

namespace Bud.Application.Features.Employees.UseCases;

public sealed class ListAvailableTeamsForEmployee(
    IEmployeeRepository employeeRepository,
    ITenantProvider tenantProvider)
{
    public async Task<Result<List<EmployeeTeamEligibleResponse>>> ExecuteAsync(
        Guid employeeId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        if (!tenantProvider.TenantId.HasValue)
        {
            return Result<List<EmployeeTeamEligibleResponse>>.NotFound(UserErrorMessages.EmployeeNotFound);
        }

        var employee = await employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (employee is null)
        {
            return Result<List<EmployeeTeamEligibleResponse>>.NotFound(UserErrorMessages.EmployeeNotFound);
        }

        var teams = await employeeRepository.GetEligibleTeamsForAssignmentAsync(
            employeeId,
            tenantProvider.TenantId.Value,
            search,
            50,
            cancellationToken);
        return Result<List<EmployeeTeamEligibleResponse>>.Success(teams.Select(t => t.ToEmployeeTeamEligibleResponse()).ToList());
    }
}
