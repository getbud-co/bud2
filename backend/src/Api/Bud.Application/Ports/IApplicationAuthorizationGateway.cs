using System.Security.Claims;

namespace Bud.Application.Ports;

public interface IApplicationAuthorizationGateway
{
    Task<bool> CanReadAsync<TResource>(ClaimsPrincipal user, TResource resource, CancellationToken cancellationToken = default);

    Task<bool> CanWriteAsync<TResource>(ClaimsPrincipal user, TResource resource, CancellationToken cancellationToken = default);
}
