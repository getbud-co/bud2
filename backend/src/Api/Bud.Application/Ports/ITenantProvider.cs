namespace Bud.Application.Ports;

public interface ITenantProvider
{
    Guid? TenantId { get; }
    Guid? EmployeeId { get; }
    bool IsGlobalAdmin { get; }
    string? UserEmail { get; }
}
