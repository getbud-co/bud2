using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Bud.Api.Authorization;

public sealed class ApplicationAuthorizationGateway(IAuthorizationService authorizationService) : IApplicationAuthorizationGateway
{
    public async Task<bool> CanReadAsync<TResource>(ClaimsPrincipal user, TResource resource, CancellationToken cancellationToken = default)
    {
        var result = await authorizationService.AuthorizeAsync(user, resource, AuthorizationPolicies.ResourceRead);
        return result.Succeeded;
    }

    public async Task<bool> CanWriteAsync<TResource>(ClaimsPrincipal user, TResource resource, CancellationToken cancellationToken = default)
    {
        var result = await authorizationService.AuthorizeAsync(user, resource, AuthorizationPolicies.ResourceWrite);
        return result.Succeeded;
    }
}
