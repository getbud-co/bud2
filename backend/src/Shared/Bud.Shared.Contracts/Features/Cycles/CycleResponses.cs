using Bud.Shared.Kernel.Enums;

namespace Bud.Shared.Contracts.Features.Cycles;

public sealed class CycleResponse
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public CycleCadence Cadence { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public CycleStatus Status { get; set; }
}
