using Bud.Application.Common;

namespace Bud.Application.Features.Employees.UseCases;

public sealed class GetEmployeeHierarchy(
    IMemberRepository employeeRepository)
{
    public async Task<Result<List<EmployeeSubordinateResponse>>> ExecuteAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        if (!await employeeRepository.ExistsAsync(employeeId, cancellationToken))
        {
            return Result<List<EmployeeSubordinateResponse>>.NotFound(UserErrorMessages.EmployeeNotFound);
        }

        var subordinates = await employeeRepository.GetSubordinatesAsync(employeeId, 5, cancellationToken);
        var childrenByLeader = subordinates
            .Where(m => m.LeaderId.HasValue)
            .GroupBy(m => m.LeaderId!.Value)
            .ToDictionary(group => group.Key, group => group.OrderBy(m => m.Employee.FullName).ToList());

        var tree = BuildTree(employeeId, childrenByLeader, 0, 5);
        return Result<List<EmployeeSubordinateResponse>>.Success(tree);
    }

    private static List<EmployeeSubordinateResponse> BuildTree(
        Guid leaderId,
        Dictionary<Guid, List<OrganizationEmployeeMember>> childrenByLeader,
        int depth,
        int maxDepth)
    {
        if (depth >= maxDepth || !childrenByLeader.TryGetValue(leaderId, out var children))
        {
            return [];
        }

        return children
            .Select(member => new EmployeeSubordinateResponse
            {
                Id = member.EmployeeId,
                FullName = member.Employee.FullName,
                Initials = GetInitials(member.Employee.FullName),
                Role = member.Role == EmployeeRole.TeamLeader ? "Líder" : "Contribuidor individual",
                Children = BuildTree(member.EmployeeId, childrenByLeader, depth + 1, maxDepth),
            })
            .ToList();
    }

    private static string GetInitials(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return "?";
        }

        if (parts.Length == 1)
        {
            return parts[0][..1].ToUpperInvariant();
        }

        return $"{parts[0][..1]}{parts[^1][..1]}".ToUpperInvariant();
    }
}
