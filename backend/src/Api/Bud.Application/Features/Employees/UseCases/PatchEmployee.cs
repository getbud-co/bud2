using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Employees.UseCases;

public sealed record PatchEmployeeCommand(
    Optional<string> FullName,
    Optional<string> Email,
    Optional<EmployeeRole> Role,
    Optional<Guid?> LeaderId);

public sealed partial class PatchEmployee(
    IEmployeeRepository employeeRepository,
    IApplicationAuthorizationGateway authorizationGateway,
    ILogger<PatchEmployee> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<Employee>> ExecuteAsync(
        ClaimsPrincipal user,
        Guid id,
        PatchEmployeeCommand command,
        CancellationToken cancellationToken = default)
    {
        LogPatchingEmployee(logger, id);

        var employee = await employeeRepository.GetByIdAsync(id, cancellationToken);
        if (employee is null)
        {
            LogEmployeePatchFailed(logger, id, "Not found");
            return Result<Employee>.NotFound(UserErrorMessages.EmployeeNotFound);
        }

        var canUpdate = await authorizationGateway.CanWriteAsync(user, new EmployeeResource(id), cancellationToken);
        if (!canUpdate)
        {
            LogEmployeePatchFailed(logger, id, "Forbidden");
            return Result<Employee>.Forbidden(UserErrorMessages.EmployeeUpdateForbidden);
        }

        var requestedEmail = command.Email.HasValue ? command.Email.Value : employee.Email;
        var requestedFullName = command.FullName.HasValue ? command.FullName.Value : employee.FullName;
        var requestedLeaderId = command.LeaderId.HasValue ? command.LeaderId.Value : employee.LeaderId;
        var requestedRole = command.Role.HasValue ? command.Role.Value : employee.Role;

        if (!EmailAddress.TryCreate(requestedEmail, out var emailAddress))
        {
            LogEmployeePatchFailed(logger, id, "Invalid email");
            return Result<Employee>.Failure(UserErrorMessages.EmployeeInvalidEmail, ErrorType.Validation);
        }

        if (!PersonName.TryCreate(requestedFullName, out var personName))
        {
            LogEmployeePatchFailed(logger, id, "Invalid name");
            return Result<Employee>.Failure(UserErrorMessages.EmployeeNameRequired, ErrorType.Validation);
        }

        if (employee.Email != emailAddress.Value)
        {
            if (!await employeeRepository.IsEmailUniqueAsync(emailAddress.Value, id, cancellationToken))
            {
                LogEmployeePatchFailed(logger, id, "Email already in use");
                return Result<Employee>.Failure(UserErrorMessages.EmployeeEmailAlreadyInUse, ErrorType.Validation);
            }
        }

        if (command.LeaderId.HasValue && requestedLeaderId.HasValue)
        {
            var leader = await employeeRepository.GetByIdAsync(requestedLeaderId.Value, cancellationToken);
            if (leader is null)
            {
                LogEmployeePatchFailed(logger, id, "Leader not found");
                return Result<Employee>.NotFound(UserErrorMessages.LeaderNotFound);
            }

            try
            {
                leader.EnsureCanLeadOrganization(employee.OrganizationId);
            }
            catch (DomainInvariantException ex)
            {
                LogEmployeePatchFailed(logger, id, ex.Message);
                return Result<Employee>.Failure(ex.Message, ErrorType.Validation);
            }
        }

        if (employee.Role == EmployeeRole.Leader &&
            requestedRole == EmployeeRole.IndividualContributor)
        {
            if (await employeeRepository.HasSubordinatesAsync(id, cancellationToken))
            {
                LogEmployeePatchFailed(logger, id, "Leader has subordinates");
                return Result<Employee>.Failure(
                    "Não é possível alterar o perfil. Este líder possui membros de equipe.",
                    ErrorType.Validation);
            }
        }

        try
        {
            employee.UpdateProfile(
                personName.Value,
                emailAddress.Value,
                requestedRole,
                requestedLeaderId,
                employee.Id);
            await unitOfWork.CommitAsync(employeeRepository.SaveChangesAsync, cancellationToken);

            LogEmployeePatched(logger, id, employee.FullName);
            return Result<Employee>.Success(employee);
        }
        catch (DomainInvariantException ex)
        {
            LogEmployeePatchFailed(logger, id, ex.Message);
            return Result<Employee>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 4043, Level = LogLevel.Information, Message = "Patching employee {EmployeeId}")]
    private static partial void LogPatchingEmployee(ILogger logger, Guid employeeId);

    [LoggerMessage(EventId = 4044, Level = LogLevel.Information, Message = "Employee patched successfully: {EmployeeId} - '{FullName}'")]
    private static partial void LogEmployeePatched(ILogger logger, Guid employeeId, string fullName);

    [LoggerMessage(EventId = 4045, Level = LogLevel.Warning, Message = "Employee patch failed for {EmployeeId}: {Reason}")]
    private static partial void LogEmployeePatchFailed(ILogger logger, Guid employeeId, string reason);
}
