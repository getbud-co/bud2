using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;
using Bud.Shared.Contracts;

namespace Bud.Application.Features.Employees.UseCases;

public sealed class GetEmployeeById(
    IEmployeeRepository employeeRepository,
    IApplicationAuthorizationGateway authorizationGateway)
{
    public async Task<Result<OrganizationEmployeeMember>> ExecuteAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken = default)
    {
        var member = await employeeRepository.GetByIdAsync(id, cancellationToken);
        if (member is null)
        {
            return Result<OrganizationEmployeeMember>.NotFound(UserErrorMessages.EmployeeNotFound);
        }

        var canRead = await authorizationGateway.CanReadAsync(user, new EmployeeResource(id), cancellationToken);
        if (!canRead)
        {
            return Result<OrganizationEmployeeMember>.Forbidden(UserErrorMessages.EmployeeNotFound);
        }

        return Result<OrganizationEmployeeMember>.Success(member);
    }
}
