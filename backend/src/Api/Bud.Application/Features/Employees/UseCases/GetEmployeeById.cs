using Bud.Application.Common;

namespace Bud.Application.Features.Employees.UseCases;

public sealed class GetEmployeeById(
    IMemberRepository employeeRepository)
{
    public async Task<Result<OrganizationEmployeeMember>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var member = await employeeRepository.GetByIdAsync(id, cancellationToken);
        if (member is null)
        {
            return Result<OrganizationEmployeeMember>.NotFound(UserErrorMessages.EmployeeNotFound);
        }

        return Result<OrganizationEmployeeMember>.Success(member);
    }
}
