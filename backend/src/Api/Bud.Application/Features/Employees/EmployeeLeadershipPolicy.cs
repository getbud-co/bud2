using Bud.Application.Common;

namespace Bud.Application.Features.Employees;

internal static class EmployeeLeadershipPolicy
{
    public static async Task<Result<T>?> ValidateLeaderForOrganizationAsync<T>(
        IEmployeeRepository employeeRepository,
        Guid leaderId,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var leader = await employeeRepository.GetByIdAsync(leaderId, cancellationToken);
        if (leader is null)
        {
            return Result<T>.NotFound(UserErrorMessages.LeaderNotFound);
        }

        try
        {
            leader.EnsureCanLeadOrganization(organizationId);
            return null;
        }
        catch (DomainInvariantException ex)
        {
            return Result<T>.Failure(ex.Message, ErrorType.Validation);
        }
    }
}
