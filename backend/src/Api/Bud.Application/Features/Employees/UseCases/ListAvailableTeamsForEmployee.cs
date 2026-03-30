using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;

namespace Bud.Application.Features.Employees.UseCases;

public sealed class ListAvailableTeamsForEmployee(
    IEmployeeRepository employeeRepository,
    IApplicationAuthorizationGateway authorizationGateway)
{
    public async Task<Result<List<EmployeeTeamEligibleResponse>>> ExecuteAsync(
        ClaimsPrincipal user,
        Guid employeeId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (employee is null)
        {
            return Result<List<EmployeeTeamEligibleResponse>>.NotFound(UserErrorMessages.EmployeeNotFound);
        }

        var canRead = await authorizationGateway.CanReadAsync(user, new EmployeeResource(employeeId), cancellationToken);
        if (!canRead)
        {
            return Result<List<EmployeeTeamEligibleResponse>>.Forbidden(UserErrorMessages.EmployeeNotFound);
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
