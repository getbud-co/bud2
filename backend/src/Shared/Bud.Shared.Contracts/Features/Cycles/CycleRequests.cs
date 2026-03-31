using Bud.Shared.Kernel;
using Bud.Shared.Kernel.Enums;
using System.Text.Json.Serialization;

namespace Bud.Shared.Contracts.Features.Cycles;

public sealed class CreateCycleRequest
{
    public string Name { get; set; } = string.Empty;
    public CycleCadence Cadence { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public CycleStatus Status { get; set; }
}

public sealed class PatchCycleRequest
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<string> Name { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<CycleCadence> Cadence { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<DateTime> StartDate { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<DateTime> EndDate { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<CycleStatus> Status { get; set; }
}
