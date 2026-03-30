
namespace Bud.Application.Features.Sessions;

public sealed class LoginResult
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsGlobalAdmin { get; set; }
    public Guid? EmployeeId { get; set; }
    public EmployeeRole? Role { get; set; }
    public Guid? OrganizationId { get; set; }
}
