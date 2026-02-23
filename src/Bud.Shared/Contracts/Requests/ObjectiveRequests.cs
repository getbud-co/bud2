using Bud.Shared.Contracts.Common;
using System.Text.Json.Serialization;

namespace Bud.Shared.Contracts.Requests;

public sealed class CreateObjectiveRequest
{
    public Guid MissionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Dimension { get; set; }
}

public sealed class PatchObjectiveRequest
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<string> Name { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<string?> Description { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<string?> Dimension { get; set; }
}
