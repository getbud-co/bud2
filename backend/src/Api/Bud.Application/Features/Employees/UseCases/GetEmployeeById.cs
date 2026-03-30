using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;
using Bud.Shared.Contracts;

namespace Bud.Application.Features.Employees.UseCases;

public sealed class GetEmployeeById(
    IEmployeeRepository employeeRepository,
    IApplicationAuthorizationGateway authorizationGateway)
{
    public async Task<Result<Employee>> ExecuteAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetByIdAsync(id, cancellationToken);
        if (employee is null)
        {
            return Result<Employee>.NotFound(UserErrorMessages.EmployeeNotFound);
        }

        var canRead = await authorizationGateway.CanReadAsync(user, new EmployeeResource(id), cancellationToken);
        if (!canRead)
        {
            return Result<Employee>.Forbidden(UserErrorMessages.EmployeeNotFound);
        }

        return Result<Employee>.Success(employee);
    }
}
