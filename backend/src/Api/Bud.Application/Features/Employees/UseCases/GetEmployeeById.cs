using Bud.Application.Common;

namespace Bud.Application.Features.Employees.UseCases;

public sealed class GetEmployeeById(
    IEmployeeRepository employeeRepository)
{
    public async Task<Result<Employee>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetByIdAsync(id, cancellationToken);
        if (employee is null)
        {
            return Result<Employee>.NotFound(UserErrorMessages.EmployeeNotFound);
        }

        return Result<Employee>.Success(employee);
    }
}
