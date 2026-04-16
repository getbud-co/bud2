using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Tags.UseCases;

public sealed record CreateTagCommand(string Name, TeamColor Color);

public sealed partial class CreateTag(
    ITagRepository tagRepository,
    IEmployeeRepository employeeRepository,
    ITenantProvider tenantProvider,
    ILogger<CreateTag> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<Tag>> ExecuteAsync(
        CreateTagCommand command,
        CancellationToken cancellationToken = default)
    {
        LogCreating(logger, command.Name);

        var organizationId = tenantProvider.TenantId;
        if (!organizationId.HasValue)
        {
            LogCreationFailed(logger, command.Name, "Tenant not selected");
            return Result<Tag>.Forbidden(UserErrorMessages.TagCreateForbidden);
        }

        if (!tenantProvider.EmployeeId.HasValue)
        {
            LogCreationFailed(logger, command.Name, "Employee not identified");
            return Result<Tag>.Forbidden(UserErrorMessages.TagCreateForbidden);
        }

        var currentMember = await employeeRepository.GetByIdAsync(tenantProvider.EmployeeId.Value, cancellationToken);
        if (currentMember is null || !currentMember.HasMinimumRoleIn(organizationId.Value, EmployeeRole.HRManager))
        {
            LogCreationFailed(logger, command.Name, "Insufficient role");
            return Result<Tag>.Forbidden(UserErrorMessages.TagCreateForbidden);
        }

        var isNameUnique = await tagRepository.IsNameUniqueAsync(command.Name, organizationId.Value, null, cancellationToken);
        if (!isNameUnique)
        {
            LogCreationFailed(logger, command.Name, "Name already in use");
            return Result<Tag>.Failure(UserErrorMessages.TagNameConflict, ErrorType.Conflict);
        }

        try
        {
            var tag = Tag.Create(Guid.NewGuid(), organizationId.Value, command.Name, command.Color);
            await tagRepository.AddAsync(tag, cancellationToken);
            await unitOfWork.CommitAsync(tagRepository.SaveChangesAsync, cancellationToken);

            LogCreated(logger, tag.Id, tag.Name);
            return Result<Tag>.Success(tag);
        }
        catch (DomainInvariantException ex)
        {
            LogCreationFailed(logger, command.Name, ex.Message);
            return Result<Tag>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 5000, Level = LogLevel.Information, Message = "Creating tag '{Name}'")]
    private static partial void LogCreating(ILogger logger, string name);

    [LoggerMessage(EventId = 5001, Level = LogLevel.Information, Message = "Tag created successfully: {TagId} - '{Name}'")]
    private static partial void LogCreated(ILogger logger, Guid tagId, string name);

    [LoggerMessage(EventId = 5002, Level = LogLevel.Warning, Message = "Tag creation failed for '{Name}': {Reason}")]
    private static partial void LogCreationFailed(ILogger logger, string name, string reason);
}
