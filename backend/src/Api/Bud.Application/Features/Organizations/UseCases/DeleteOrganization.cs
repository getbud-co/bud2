using Bud.Application.Common;
using Bud.Application.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bud.Application.Features.Organizations.UseCases;

public sealed partial class DeleteOrganization(
    IOrganizationRepository organizationRepository,
    IOptions<GlobalAdminSettings> globalAdminSettings,
    ILogger<DeleteOrganization> logger,
    IUnitOfWork? unitOfWork = null)
{
    private readonly OrganizationDomainName? _globalAdminOrgName =
        OrganizationDomainName.TryCreate(globalAdminSettings.Value.OrganizationName, out var organizationDomainName)
            ? organizationDomainName
            : null;

    public async Task<Result> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        LogDeletingOrganization(logger, id);

        var organization = await organizationRepository.GetByIdAsync(id, cancellationToken);
        if (organization is null)
        {
            LogOrganizationDeletionFailed(logger, id, "Not found");
            return Result.NotFound(UserErrorMessages.OrganizationNotFound);
        }

        if (OrganizationProtectionPolicy.IsProtectedOrganization(organization.Name, _globalAdminOrgName))
        {
            LogOrganizationDeletionFailed(logger, id, "Protected organization");
            return Result.Failure(
                "Esta organização está protegida e não pode ser excluída.",
                ErrorType.Conflict);
        }

        if (await organizationRepository.HasEmployeesAsync(id, cancellationToken))
        {
            LogOrganizationDeletionFailed(logger, id, "Has employees");
            return Result.Failure(
                "Não é possível excluir a organização porque ela possui colaboradores associados. Remova os colaboradores primeiro.",
                ErrorType.Conflict);
        }

        await organizationRepository.RemoveAsync(organization, cancellationToken);
        await unitOfWork.CommitAsync(organizationRepository.SaveChangesAsync, cancellationToken);

        LogOrganizationDeleted(logger, id);
        return Result.Success();
    }

    [LoggerMessage(EventId = 4016, Level = LogLevel.Information, Message = "Deleting organization {OrganizationId}")]
    private static partial void LogDeletingOrganization(ILogger logger, Guid organizationId);

    [LoggerMessage(EventId = 4017, Level = LogLevel.Information, Message = "Organization deleted successfully: {OrganizationId}")]
    private static partial void LogOrganizationDeleted(ILogger logger, Guid organizationId);

    [LoggerMessage(EventId = 4018, Level = LogLevel.Warning, Message = "Organization deletion failed for {OrganizationId}: {Reason}")]
    private static partial void LogOrganizationDeletionFailed(ILogger logger, Guid organizationId, string reason);
}
