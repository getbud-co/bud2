using Bud.Application.Common;
using Bud.Application.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bud.Application.Features.Organizations.UseCases;

public sealed record PatchOrganizationCommand(
    Optional<string> Name,
    Optional<string> Cnpj,
    Optional<OrganizationPlan> Plan,
    Optional<OrganizationContractStatus> ContractStatus,
    Optional<string?> IconUrl);

public sealed partial class UpdateOrganization(
    IOrganizationRepository organizationRepository,
    IOptions<GlobalAdminSettings> globalAdminSettings,
    ILogger<UpdateOrganization> logger,
    IUnitOfWork? unitOfWork = null)
{
    private readonly OrganizationDomainName? _globalAdminOrgName =
        OrganizationDomainName.TryCreate(globalAdminSettings.Value.OrganizationName, out var organizationDomainName)
            ? organizationDomainName
            : null;

    public async Task<Result<Organization>> ExecuteAsync(
        Guid id,
        UpdateOrganizationCommand command,
        CancellationToken cancellationToken = default)
    {
        LogUpdatingOrganization(logger, id);

        var organization = await organizationRepository.GetByIdAsync(id, cancellationToken);
        if (organization is null)
        {
            LogOrganizationUpdateFailed(logger, id, "Not found");
            return Result<Organization>.NotFound(UserErrorMessages.OrganizationNotFound);
        }

        if (OrganizationProtectionPolicy.IsProtectedOrganization(organization.Name, _globalAdminOrgName))
        {
            LogOrganizationUpdateFailed(logger, id, "Protected organization");
            return Result<Organization>.Failure(
                "Esta organização está protegida e não pode ser alterada.",
                ErrorType.Conflict);
        }

        try
        {
            if (command.Name.HasValue)
            {
                organization.Rename(command.Name.Value ?? string.Empty);
            }
            if (command.Cnpj.HasValue)
            {
                organization.Cnpj = command.Cnpj.Value ?? string.Empty;
            }

            if (command.Plan.HasValue)
            {
                organization.Plan = command.Plan.Value;
            }

            if (command.ContractStatus.HasValue)
            {
                organization.ContractStatus = command.ContractStatus.Value;
            }

            if (command.IconUrl.HasValue)
            {
                organization.IconUrl = command.IconUrl.Value;
            }

            await unitOfWork.CommitAsync(organizationRepository.SaveChangesAsync, cancellationToken);

            LogOrganizationUpdated(logger, id, organization.Name.Value);
            return Result<Organization>.Success(organization);
        }
        catch (DomainInvariantException ex)
        {
            LogOrganizationUpdateFailed(logger, id, ex.Message);
            return Result<Organization>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 4013, Level = LogLevel.Information, Message = "Updating organization {OrganizationId}")]
    private static partial void LogUpdatingOrganization(ILogger logger, Guid organizationId);

    [LoggerMessage(EventId = 4014, Level = LogLevel.Information, Message = "Organization updated successfully: {OrganizationId} - '{Name}'")]
    private static partial void LogOrganizationUpdated(ILogger logger, Guid organizationId, string name);

    [LoggerMessage(EventId = 4015, Level = LogLevel.Warning, Message = "Organization update failed for {OrganizationId}: {Reason}")]
    private static partial void LogOrganizationUpdateFailed(ILogger logger, Guid organizationId, string reason);
}
