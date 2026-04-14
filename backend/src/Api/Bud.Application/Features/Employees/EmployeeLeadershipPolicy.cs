using Bud.Application.Common;

namespace Bud.Application.Features.Employees;

internal static class EmployeeLeadershipPolicy
{
    public static async Task<Result<T>?> ValidateLeaderForOrganizationAsync<T>(
        IMemberRepository employeeRepository,
        Guid leaderId,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var leaderMember = await employeeRepository.GetByIdAsync(leaderId, cancellationToken);
        if (leaderMember is null)
        {
            return Result<T>.NotFound(UserErrorMessages.LeaderNotFound);
        }

        try
        {
            leaderMember.EnsureCanLeadOrganization(organizationId);
            return null;
        }
        catch (DomainInvariantException ex)
        {
            return Result<T>.Failure(ex.Message, ErrorType.Validation);
        }
    }
}
