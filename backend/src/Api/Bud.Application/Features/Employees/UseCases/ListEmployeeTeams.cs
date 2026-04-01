using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;

namespace Bud.Application.Features.Employees.UseCases;

public sealed class ListEmployeeTeams(
    IEmployeeRepository employeeRepository,
    IApplicationAuthorizationGateway authorizationGateway)
{
    public async Task<Result<List<EmployeeTeamResponse>>> ExecuteAsync(
        ClaimsPrincipal user,
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        if (!await employeeRepository.ExistsAsync(employeeId, cancellationToken))
        {
            return Result<List<EmployeeTeamResponse>>.NotFound(UserErrorMessages.EmployeeNotFound);
        }

        var canRead = await authorizationGateway.CanReadAsync(user, new EmployeeResource(employeeId), cancellationToken);
        if (!canRead)
        {
            return Result<List<EmployeeTeamResponse>>.Forbidden(UserErrorMessages.EmployeeNotFound);
        }

        var teams = await employeeRepository.GetTeamsAsync(employeeId, cancellationToken);
        return Result<List<EmployeeTeamResponse>>.Success(teams.Select(t => t.ToEmployeeTeamResponse()).ToList());
    }
}
