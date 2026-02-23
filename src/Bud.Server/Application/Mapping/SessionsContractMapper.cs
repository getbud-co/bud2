using Bud.Server.Application.ReadModels;

namespace Bud.Server.Application.Mapping;

internal static class SessionsContractMapper
{
    public static SessionResponse ToResponse(this LoginResult source)
    {
        return new SessionResponse
        {
            Token = source.Token,
            Email = source.Email,
            DisplayName = source.DisplayName,
            IsGlobalAdmin = source.IsGlobalAdmin,
            CollaboratorId = source.CollaboratorId,
            Role = source.Role,
            OrganizationId = source.OrganizationId
        };
    }
}
