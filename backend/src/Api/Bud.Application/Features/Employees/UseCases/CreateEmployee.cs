using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Employees.UseCases;

public sealed record CreateEmployeeCommand(
    string FullName,
    string Email,
    EmployeeRole Role);

public sealed partial class CreateEmployee(
    IEmployeeRepository employeeRepository,
    ITenantProvider tenantProvider,
    ILogger<CreateEmployee> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<Employee>> ExecuteAsync(
        CreateEmployeeCommand command,
        CancellationToken cancellationToken = default)
    {
        LogCreatingEmployee(logger, command.FullName);

        var organizationId = tenantProvider.TenantId;
        if (!organizationId.HasValue)
        {
            LogEmployeeCreationFailed(logger, command.FullName, "Organization context not found");
            return Result<Employee>.Failure("Contexto de organização não encontrado.", ErrorType.Validation);
        }

        if (!EmailAddress.TryCreate(command.Email, out var emailAddress))
        {
            LogEmployeeCreationFailed(logger, command.FullName, "Invalid email");
            return Result<Employee>.Failure("E-mail inválido.", ErrorType.Validation);
        }

        if (!EmployeeName.TryCreate(command.FullName, out var employeeName))
        {
            LogEmployeeCreationFailed(logger, command.FullName, "Invalid full name");
            return Result<Employee>.Failure("O nome do colaborador é obrigatório.", ErrorType.Validation);
        }

        if (!await employeeRepository.IsEmailUniqueAsync(emailAddress, null, cancellationToken))
        {
            LogEmployeeCreationFailed(logger, command.FullName, "Email already in use");
            return Result<Employee>.Failure("E-mail já está em uso.", ErrorType.Validation);
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

            var newId = Guid.NewGuid();
            var employee = Employee.Create(newId, personName.Value, emailAddress.Value);
            var member = Membership.Create(newId, organizationId.Value, command.Role, command.LeaderId);
            member.Employee = employee;
            employee.Memberships.Add(member);

            if (command.TeamId.HasValue)
            {
                employee.EmployeeTeams.Add(new EmployeeTeam
                {
                    EmployeeId = newId,
                    TeamId = command.TeamId.Value,
                    AssignedAt = DateTime.UtcNow,
                });
            }

            await employeeRepository.AddAsync(employee, cancellationToken);
            await unitOfWork.CommitAsync(employeeRepository.SaveChangesAsync, cancellationToken);

            LogEmployeeCreated(logger, newId, employee.FullName);
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
