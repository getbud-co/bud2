namespace Bud.Application.Features.Employees.UseCases;

public sealed class GetEmployeeById(IEmployeeRepository employeeRepository)
{
    public async Task<Result<Employee>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetByIdAsync(id, cancellationToken);
        return employee is null
            ? Result<Employee>.NotFound("Funcionário não encontrado.")
            : Result<Employee>.Success(employee);
    }
}
