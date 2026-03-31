using Bud.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Bud.Application.Common;

namespace Bud.Infrastructure.Features.Organizations;

public sealed class OrganizationAuthorizationService(
    ApplicationDbContext dbContext,
    ITenantProvider tenantProvider) : IOrganizationAuthorizationService
{
    public async Task<Result> RequireOrgAdminAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        if (tenantProvider.IsGlobalAdmin)
        {
            return Result.Success();
        }

        if (string.IsNullOrEmpty(tenantProvider.UserEmail))
        {
            return Result.Forbidden("Colaborador não identificado.");
        }

        var isOrgAdmin = await dbContext.Collaborators
            .AnyAsync(c =>
                c.OrganizationId == organizationId &&
                c.Email == tenantProvider.UserEmail &&
                c.Role == CollaboratorRole.OrgAdmin,
                cancellationToken);

        return isOrgAdmin
            ? Result.Success()
            : Result.Forbidden("Apenas administradores da organização podem realizar esta ação.");
    }

    public async Task<Result> RequireWriteAccessAsync(
        Guid organizationId,
        Guid resourceId,
        CancellationToken cancellationToken = default)
    {
        if (tenantProvider.IsGlobalAdmin)
        {
            return Result.Success();
        }

        if (string.IsNullOrEmpty(tenantProvider.UserEmail))
        {
            return Result.Forbidden("Colaborador não identificado.");
        }

        var isOrgAdmin = await dbContext.Collaborators
            .AnyAsync(c =>
                c.OrganizationId == organizationId &&
                c.Email == tenantProvider.UserEmail &&
                c.Role == CollaboratorRole.OrgAdmin,
                cancellationToken);

        return isOrgAdmin
            ? Result.Success()
            : Result.Forbidden("Você não tem permissão de escrita nesta organização.");
    }
}
