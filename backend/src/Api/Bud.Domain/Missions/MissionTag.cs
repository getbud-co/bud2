namespace Bud.Domain.Tags;

public sealed class MissionTag
{
    public Guid MissionId { get; set; }
    public Mission Mission { get; set; } = null!;

    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
