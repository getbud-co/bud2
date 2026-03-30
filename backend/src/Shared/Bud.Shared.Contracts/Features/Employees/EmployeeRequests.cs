using Bud.Shared.Kernel;
using System.Text.Json.Serialization;

namespace Bud.Shared.Contracts.Features.Employees;

public sealed class CreateEmployeeRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public EmployeeRole Role { get; set; } = EmployeeRole.IndividualContributor;
    public Guid? TeamId { get; set; }
    public Guid? LeaderId { get; set; }
}

public sealed class PatchEmployeeRequest
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<string> FullName { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<string> Email { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<EmployeeRole> Role { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<Guid?> LeaderId { get; set; }
}

public sealed class PatchEmployeeTeamsRequest
{
    public List<Guid> TeamIds { get; set; } = [];
}
