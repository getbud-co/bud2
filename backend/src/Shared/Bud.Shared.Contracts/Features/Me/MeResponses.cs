namespace Bud.Shared.Contracts.Features.Me;

public sealed class MyOrganizationResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
