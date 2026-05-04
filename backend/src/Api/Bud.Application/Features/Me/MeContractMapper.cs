
namespace Bud.Application.Features.Me;

internal static class MeContractMapper
{
    public static MyOrganizationResponse ToResponse(this OrganizationSnapshot source)
    {
        return new MyOrganizationResponse
        {
            Id = source.Id,
            Name = source.Name
        };
    }
}
