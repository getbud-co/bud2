using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Employees.UseCases;

public sealed record UpdateEmployeeCommand(
    Optional<string> FullName,
    Optional<string> Email,
    Optional<EmployeeRole> Role);

public sealed partial class UpdateEmployee(
    IEmployeeRepository employeeRepository,
    ILogger<UpdateEmployee> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result<Employee>> ExecuteAsync(
        Guid id,
        UpdateEmployeeCommand command,
        CancellationToken cancellationToken = default)
    {
        LogUpdatingEmployee(logger, id);

        var employee = await employeeRepository.GetByIdAsync(id, cancellationToken);
        if (employee is null)
        {
            LogEmployeeUpdateFailed(logger, id, "Not found");
            return Result<Employee>.NotFound("Funcionário não encontrado.");
        }

        var requestedEmail = command.Email.HasValue ? command.Email.Value : employee.Email;
        var requestedFullName = command.FullName.HasValue ? command.FullName.Value : employee.FullName;
        var requestedRole = command.Role.HasValue ? command.Role.Value : employee.Role;

        if (!EmailAddress.TryCreate(requestedEmail, out var emailAddress))
        {
            LogEmployeeUpdateFailed(logger, id, "Invalid email");
            return Result<Employee>.Failure("E-mail inválido.", ErrorType.Validation);
        }

        if (employee.Email != emailAddress.Value && !await employeeRepository.IsEmailUniqueAsync(emailAddress.Value, id, cancellationToken))
        {
            LogEmployeeUpdateFailed(logger, id, "Email already in use");
            return Result<Employee>.Failure("E-mail já está em uso.", ErrorType.Validation);
        }

        try
        {
            employee.UpdateProfile(requestedFullName!, emailAddress.Value, requestedRole);
            await unitOfWork.CommitAsync(employeeRepository.SaveChangesAsync, cancellationToken);

            LogEmployeeUpdated(logger, id, employee.FullName);
            return Result<Employee>.Success(employee);
        }
        catch (DomainInvariantException ex)
        {
            LogEmployeeUpdateFailed(logger, id, ex.Message);
            return Result<Employee>.Failure(ex.Message, ErrorType.Validation);
        }
    }

    [LoggerMessage(EventId = 4043, Level = LogLevel.Information, Message = "Updating employee {EmployeeId}")]
    private static partial void LogUpdatingEmployee(ILogger logger, Guid employeeId);

    [LoggerMessage(EventId = 4044, Level = LogLevel.Information, Message = "Employee updated successfully: {EmployeeId} - '{FullName}'")]
    private static partial void LogEmployeeUpdated(ILogger logger, Guid employeeId, string fullName);

    [LoggerMessage(EventId = 4045, Level = LogLevel.Warning, Message = "Employee update failed for {EmployeeId}: {Reason}")]
    private static partial void LogEmployeeUpdateFailed(ILogger logger, Guid employeeId, string reason);
}
