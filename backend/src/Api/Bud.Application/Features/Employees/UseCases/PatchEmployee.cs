using Bud.Application.Common;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Employees.UseCases;

public sealed record PatchEmployeeCommand(
    Optional<string> FullName,
    Optional<string> Email,
    Optional<string?> Nickname,
    Optional<EmployeeLanguage> Language,
    Optional<EmployeeRole> Role,
    Optional<Guid?> LeaderId,
    Optional<EmployeeStatus> Status);

public sealed partial class PatchEmployee(
    IMemberRepository employeeRepository,
    ILogger<PatchEmployee> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<OrganizationEmployeeMember>> ExecuteAsync(
        Guid id,
        PatchEmployeeCommand command,
        CancellationToken cancellationToken = default)
    {
        LogPatchingEmployee(logger, id);

        var member = await employeeRepository.GetByIdAsync(id, cancellationToken);
        if (member is null)
        {
            LogEmployeePatchFailed(logger, id, "Not found");
            return Result<OrganizationEmployeeMember>.NotFound(UserErrorMessages.EmployeeNotFound);
        }

        var requestedEmail = command.Email.HasValue ? command.Email.Value : member.Employee.Email;
        var requestedFullName = command.FullName.HasValue ? command.FullName.Value : member.Employee.FullName;
        var requestedNickname = command.Nickname.HasValue ? command.Nickname.Value : member.Employee.Nickname;
        var requestedLanguage = command.Language.HasValue ? command.Language.Value : member.Employee.Language;
        var requestedLeaderId = command.LeaderId.HasValue ? command.LeaderId.Value : member.LeaderId;
        var requestedRole = command.Role.HasValue ? command.Role.Value : member.Role;
        var requestedStatus = command.Status.HasValue ? command.Status.Value : member.Employee.Status;

        if (!EmailAddress.TryCreate(requestedEmail, out var emailAddress))
        {
            LogEmployeePatchFailed(logger, id, "Invalid email");
            return Result<OrganizationEmployeeMember>.Failure(UserErrorMessages.EmployeeInvalidEmail, ErrorType.Validation);
        }

        if (!PersonName.TryCreate(requestedFullName, out var personName))
        {
            LogEmployeePatchFailed(logger, id, "Invalid name");
            return Result<OrganizationEmployeeMember>.Failure(UserErrorMessages.EmployeeNameRequired, ErrorType.Validation);
        }

        if (member.Employee.Email != emailAddress.Value)
        {
            if (!await employeeRepository.IsEmailUniqueAsync(emailAddress.Value, id, cancellationToken))
            {
                LogEmployeePatchFailed(logger, id, "Email already in use");
                return Result<OrganizationEmployeeMember>.Failure(UserErrorMessages.EmployeeEmailAlreadyInUse, ErrorType.Validation);
            }
        }

        if (command.LeaderId.HasValue && requestedLeaderId.HasValue)
        {
            var leaderMember = await employeeRepository.GetByIdAsync(requestedLeaderId.Value, cancellationToken);
            if (leaderMember is null)
            {
                LogEmployeePatchFailed(logger, id, "Leader not found");
                return Result<OrganizationEmployeeMember>.NotFound(UserErrorMessages.LeaderNotFound);
            }

            try
            {
                leaderMember.EnsureCanLeadOrganization(member.OrganizationId);
            }
            catch (DomainInvariantException ex)
            {
                LogEmployeePatchFailed(logger, id, ex.Message);
                return Result<OrganizationEmployeeMember>.Failure(ex.Message, ErrorType.Validation);
            }
        }

        if (member.Role == EmployeeRole.TeamLeader &&
            requestedRole == EmployeeRole.Contributor)
        {
            if (await employeeRepository.HasSubordinatesAsync(id, cancellationToken))
            {
                LogEmployeePatchFailed(logger, id, "Leader has subordinates");
                return Result<OrganizationEmployeeMember>.Failure(
                    "Não é possível alterar o perfil. Este líder possui membros de equipe.",
                    ErrorType.Validation);
            }
        }

        try
        {
            member.Employee.UpdateIdentity(personName.Value, emailAddress.Value);
            member.Employee.Nickname = requestedNickname;
            member.Employee.Language = requestedLanguage;
            member.Employee.Status = requestedStatus;
            member.UpdateProfile(requestedRole, requestedLeaderId, member.EmployeeId);
            await unitOfWork.CommitAsync(employeeRepository.SaveChangesAsync, cancellationToken);

            LogEmployeePatched(logger, id, member.Employee.FullName);
            return Result<OrganizationEmployeeMember>.Success(member);
        }
        catch (DomainInvariantException ex)
        {
            LogEmployeePatchFailed(logger, id, ex.Message);
            return Result<OrganizationEmployeeMember>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 4043, Level = LogLevel.Information, Message = "Patching employee {EmployeeId}")]
    private static partial void LogPatchingEmployee(ILogger logger, Guid employeeId);

    [LoggerMessage(EventId = 4044, Level = LogLevel.Information, Message = "Employee patched successfully: {EmployeeId} - '{FullName}'")]
    private static partial void LogEmployeePatched(ILogger logger, Guid employeeId, string fullName);

    [LoggerMessage(EventId = 4045, Level = LogLevel.Warning, Message = "Employee patch failed for {EmployeeId}: {Reason}")]
    private static partial void LogEmployeePatchFailed(ILogger logger, Guid employeeId, string reason);
}
