using Bud.Application.Common;

namespace Bud.Application.Features.Employees.UseCases;

public sealed class ListEmployeeTeams(
    IEmployeeRepository employeeRepository)
{
    public async Task<Result<List<EmployeeTeamResponse>>> ExecuteAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        if (!await employeeRepository.ExistsAsync(employeeId, cancellationToken))
        {
            return Result<List<EmployeeTeamResponse>>.NotFound(UserErrorMessages.EmployeeNotFound);
        }

        var teams = await employeeRepository.GetTeamsAsync(employeeId, cancellationToken);
        return Result<List<EmployeeTeamResponse>>.Success(teams.Select(t => t.ToEmployeeTeamResponse()).ToList());
    }
}
