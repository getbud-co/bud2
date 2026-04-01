using System.Security.Claims;
using Bud.Application.Common;

namespace Bud.Application.Ports;

public interface IApplicationAuthorizationGateway
{
    Task<Result> AuthorizeReadAsync<TResource>(ClaimsPrincipal user, TResource resource, CancellationToken cancellationToken = default);

    Task<Result> AuthorizeWriteAsync<TResource>(ClaimsPrincipal user, TResource resource, CancellationToken cancellationToken = default);

    Task<bool> CanReadAsync<TResource>(ClaimsPrincipal user, TResource resource, CancellationToken cancellationToken = default);

    Task<bool> CanWriteAsync<TResource>(ClaimsPrincipal user, TResource resource, CancellationToken cancellationToken = default);
}
