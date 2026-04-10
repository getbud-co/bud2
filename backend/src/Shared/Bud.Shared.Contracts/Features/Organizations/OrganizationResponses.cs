using Bud.Shared.Kernel.Enums;

namespace Bud.Shared.Contracts.Features.Organizations;

public sealed class OrganizationResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public OrganizationPlan Plan { get; set; }
    public OrganizationContractStatus ContractStatus { get; set; }
    public string Cnpj { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
