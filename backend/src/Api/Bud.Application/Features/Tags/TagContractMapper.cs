using Bud.Shared.Contracts.Features.Tags;

namespace Bud.Application.Features.Tags;

public static class TagContractMapper
{
    public static TagResponse ToResponse(this Tag source, int linkedItems = 0)
    {
        return new TagResponse
        {
            Id = source.Id,
            OrganizationId = source.OrganizationId,
            Name = source.Name,
            Color = source.Color,
            LinkedItems = linkedItems,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public static TagResponse ToResponse(this TagWithCount source)
        => source.Tag.ToResponse(source.LinkedItems);
}
