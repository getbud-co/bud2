using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;
using Bud.Shared.Contracts;
using Microsoft.Extensions.Logging;

namespace Bud.Application.Features.Employees.UseCases;

public sealed partial class DeleteEmployee(
    IEmployeeRepository employeeRepository,
    IApplicationAuthorizationGateway authorizationGateway,
    ILogger<DeleteEmployee> logger,
    IUnitOfWork? unitOfWork = null)
{
    public async Task<Result> ExecuteAsync(
        ClaimsPrincipal user,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        LogDeletingEmployee(logger, id);

        var member = await employeeRepository.GetByIdAsync(id, cancellationToken);
        if (member is null)
        {
            LogEmployeeDeletionFailed(logger, id, "Not found");
            return Result.NotFound(UserErrorMessages.EmployeeNotFound);
        }

        var canDelete = await authorizationGateway.CanWriteAsync(user, new EmployeeResource(id), cancellationToken);
        if (!canDelete)
        {
            LogEmployeeDeletionFailed(logger, id, "Forbidden");
            return Result.Forbidden(UserErrorMessages.EmployeeDeleteForbidden);
        }

        if (await employeeRepository.HasSubordinatesAsync(id, cancellationToken))
        {
            LogEmployeeDeletionFailed(logger, id, "Has subordinates");
            return Result.Failure(
                "Não é possível excluir o colaborador. Ele é líder de outros colaboradores.",
                ErrorType.Conflict);
        }

        if (await employeeRepository.HasMissionsAsync(id, cancellationToken))
        {
            LogEmployeeDeletionFailed(logger, id, "Has missions");
            return Result.Failure(
                "Não é possível excluir o colaborador porque existem metas associadas a ele.",
                ErrorType.Conflict);
        }

        await employeeRepository.RemoveAsync(member, cancellationToken);
        await unitOfWork.CommitAsync(employeeRepository.SaveChangesAsync, cancellationToken);

        LogEmployeeDeleted(logger, id);
        return Result.Success();
    }

    [LoggerMessage(EventId = 4046, Level = LogLevel.Information, Message = "Deleting employee {EmployeeId}")]
    private static partial void LogDeletingEmployee(ILogger logger, Guid employeeId);

    [LoggerMessage(EventId = 4047, Level = LogLevel.Information, Message = "Employee deleted successfully: {EmployeeId}")]
    private static partial void LogEmployeeDeleted(ILogger logger, Guid employeeId);

    [LoggerMessage(EventId = 4048, Level = LogLevel.Warning, Message = "Employee deletion failed for {EmployeeId}: {Reason}")]
    private static partial void LogEmployeeDeletionFailed(ILogger logger, Guid employeeId, string reason);
}
