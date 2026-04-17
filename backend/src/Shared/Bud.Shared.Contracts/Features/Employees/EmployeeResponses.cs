namespace Bud.Shared.Contracts.Features.Employees;

public sealed class EmployeeResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public EmployeeRole Role { get; set; } = EmployeeRole.IndividualContributor;
    public Guid OrganizationId { get; set; }
    public bool IsGlobalAdmin { get; set; }
}
