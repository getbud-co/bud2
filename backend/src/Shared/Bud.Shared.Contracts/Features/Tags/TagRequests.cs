using Bud.Shared.Kernel;

namespace Bud.Shared.Contracts.Features.Tags;

public sealed class CreateTagRequest
{
    public string Name { get; set; } = string.Empty;
    public TagColor Color { get; set; }
}

public sealed class PatchTagRequest
{
    public string Name { get; set; } = string.Empty;
    public TagColor Color { get; set; }
}
