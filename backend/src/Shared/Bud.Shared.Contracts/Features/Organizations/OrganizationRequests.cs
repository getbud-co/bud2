using Bud.Shared.Kernel;
using Bud.Shared.Kernel.Enums;
using System.Text.Json.Serialization;

namespace Bud.Shared.Contracts.Features.Organizations;

public sealed class CreateOrganizationRequest
{
    public string Name { get; set; } = string.Empty;
    public OrganizationPlan Plan { get; set; } = OrganizationPlan.Free;
    public OrganizationContractStatus ContractStatus { get; set; } = OrganizationContractStatus.ToApproval;
    public string? IconUrl { get; set; }
}

public sealed class PatchOrganizationRequest
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<string> Name { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<OrganizationPlan> Plan { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<OrganizationContractStatus> ContractStatus { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<string?> IconUrl { get; set; }
}
