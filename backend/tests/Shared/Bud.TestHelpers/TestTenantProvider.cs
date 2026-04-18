using Bud.Application.Ports;

namespace Bud.TestHelpers;

public sealed class TestTenantProvider : ITenantProvider
{
    public Guid? TenantId { get; set; }
    public Guid? EmployeeId { get; set; }
    public bool IsGlobalAdmin { get; set; }
    public string? UserEmail { get; set; }
}
