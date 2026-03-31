using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Employees.UseCases;

public sealed record CreateEmployeeCommand(
    string FullName,
    string Email,
    EmployeeRole Role,
    Guid? TeamId,
    Guid? LeaderId);

public sealed partial class CreateEmployee(
    IEmployeeRepository employeeRepository,
    IApplicationAuthorizationGateway authorizationGateway,
    ITenantProvider tenantProvider,
    ILogger<CreateEmployee> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<Employee>> ExecuteAsync(
        ClaimsPrincipal user,
        CreateEmployeeCommand command,
        CancellationToken cancellationToken = default)
    {
        LogCreatingEmployee(logger, command.FullName);

        var organizationId = tenantProvider.TenantId;
        if (!organizationId.HasValue)
        {
            LogEmployeeCreationFailed(logger, command.FullName, "Organization context not found");
            return Result<Employee>.Failure(UserErrorMessages.EmployeeContextNotFound, ErrorType.Validation);
        }

        var canCreate = await authorizationGateway.CanWriteAsync(
            user,
            new CreateEmployeeContext(organizationId.Value),
            cancellationToken);
        if (!canCreate)
        {
            LogEmployeeCreationFailed(logger, command.FullName, "Forbidden");
            return Result<Employee>.Forbidden(UserErrorMessages.EmployeeCreateForbidden);
        }

        if (!EmailAddress.TryCreate(command.Email, out var emailAddress))
        {
            LogEmployeeCreationFailed(logger, command.FullName, "Invalid email");
            return Result<Employee>.Failure(UserErrorMessages.EmployeeInvalidEmail, ErrorType.Validation);
        }

        if (!PersonName.TryCreate(command.FullName, out var personName))
        {
            LogEmployeeCreationFailed(logger, command.FullName, "Invalid name");
            return Result<Employee>.Failure(UserErrorMessages.EmployeeNameRequired, ErrorType.Validation);
        }

        try
        {
            if (command.TeamId.HasValue)
            {
                var validTeams = await employeeRepository.CountTeamsByIdsAndOrganizationAsync(
                    [command.TeamId.Value],
                    organizationId.Value,
                    cancellationToken);

                if (validTeams != 1)
                {
                    LogEmployeeCreationFailed(logger, command.FullName, "Team not found");
                    return Result<Employee>.NotFound(UserErrorMessages.TeamNotFound);
                }
            }

            var employee = Employee.Create(
                Guid.NewGuid(),
                organizationId.Value,
                personName.Value,
                emailAddress.Value,
                command.Role,
                command.LeaderId);

            if (command.TeamId.HasValue)
            {
                employee.EmployeeTeams.Add(new EmployeeTeam
                {
                    EmployeeId = employee.Id,
                    TeamId = command.TeamId.Value,
                    AssignedAt = DateTime.UtcNow
                });
            }

            await employeeRepository.AddAsync(employee, cancellationToken);
            await unitOfWork.CommitAsync(employeeRepository.SaveChangesAsync, cancellationToken);

            LogEmployeeCreated(logger, employee.Id, employee.FullName);
            return Result<Employee>.Success(employee);
        }
        catch (DomainInvariantException ex)
        {
            LogEmployeeCreationFailed(logger, command.FullName, ex.Message);
            return Result<Employee>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 4040, Level = LogLevel.Information, Message = "Creating employee '{FullName}'")]
    private static partial void LogCreatingEmployee(ILogger logger, string fullName);

    [LoggerMessage(EventId = 4041, Level = LogLevel.Information, Message = "Employee created successfully: {EmployeeId} - '{FullName}'")]
    private static partial void LogEmployeeCreated(ILogger logger, Guid employeeId, string fullName);

    [LoggerMessage(EventId = 4042, Level = LogLevel.Warning, Message = "Employee creation failed for '{FullName}': {Reason}")]
    private static partial void LogEmployeeCreationFailed(ILogger logger, string fullName, string reason);
}
