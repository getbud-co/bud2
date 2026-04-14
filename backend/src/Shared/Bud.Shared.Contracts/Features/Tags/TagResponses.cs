using Bud.Shared.Kernel;

namespace Bud.Shared.Contracts.Features.Tags;

public sealed class TagResponse
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public TeamColor Color { get; set; }
    public int LinkedItems { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
