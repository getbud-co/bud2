using Bud.Application.Common;

namespace Bud.Application.Features.Employees.UseCases;

public sealed class ListAvailableTeamsForEmployee(
    IEmployeeRepository employeeRepository)
{
    public async Task<Result<List<EmployeeTeamEligibleResponse>>> ExecuteAsync(
        Guid employeeId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (employee is null)
        {
            return Result<List<EmployeeTeamEligibleResponse>>.NotFound(UserErrorMessages.EmployeeNotFound);
        }

        var teams = await employeeRepository.GetEligibleTeamsForAssignmentAsync(
            employeeId,
            employee.OrganizationId,
            search,
            50,
            cancellationToken);
        return Result<List<EmployeeTeamEligibleResponse>>.Success(teams.Select(t => t.ToEmployeeTeamEligibleResponse()).ToList());
    }
}
