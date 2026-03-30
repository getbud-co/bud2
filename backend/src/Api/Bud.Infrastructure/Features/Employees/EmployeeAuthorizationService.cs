using Bud.Application.Common;
using Bud.Application.Features.Employees;
using Bud.Application.Ports;
using Bud.Infrastructure.Authorization;
using Bud.Infrastructure.Persistence;

namespace Bud.Infrastructure.Features.Employees;

public sealed class EmployeeAuthorizationService(
    IEmployeeRepository employeeRepository,
    ApplicationDbContext dbContext,
    ITenantProvider tenantProvider)
    : IReadAuthorizationRule<EmployeeResource>,
      IWriteAuthorizationRule<EmployeeResource>,
      IWriteAuthorizationRule<CreateEmployeeContext>
{
    public async Task<Result> EvaluateAsync(EmployeeResource resource, CancellationToken cancellationToken = default)
        => await TenantScopedAuthorization.AuthorizeReadAsync(
            tenantProvider,
            ct => employeeRepository.GetByIdAsync(resource.EmployeeId, ct),
            employee => employee.OrganizationId,
            "Colaborador não encontrado.",
            "Você não tem permissão para acessar este colaborador.",
            cancellationToken);

    async Task<Result> IWriteAuthorizationRule<EmployeeResource>.EvaluateAsync(EmployeeResource resource, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(resource.EmployeeId, cancellationToken);
        if (employee is null)
        {
            return Result.NotFound("Colaborador não encontrado.");
        }

        return await RequireLeaderAccessAsync(employee.OrganizationId, cancellationToken);
    }

    Task<Result> IWriteAuthorizationRule<CreateEmployeeContext>.EvaluateAsync(CreateEmployeeContext context, CancellationToken cancellationToken)
        => RequireLeaderAccessAsync(context.OrganizationId, cancellationToken);

    private Task<Result> RequireLeaderAccessAsync(Guid organizationId, CancellationToken cancellationToken)
        => LeaderScopedAuthorization.RequireLeaderInOrganizationAsync(
            dbContext,
            tenantProvider,
            organizationId,
            "Colaborador não identificado.",
            "Apenas um líder da organização pode realizar esta ação.",
            cancellationToken);
}
