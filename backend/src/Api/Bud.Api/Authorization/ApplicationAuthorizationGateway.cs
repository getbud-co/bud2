using System.Security.Claims;
using Bud.Api.Authorization.Handlers;
using Bud.Application.Common;
using Microsoft.AspNetCore.Authorization;

namespace Bud.Api.Authorization;

public sealed class ApplicationAuthorizationGateway(
    IAuthorizationService authorizationService,
    IServiceProvider? serviceProvider = null) : IApplicationAuthorizationGateway
{
    public async Task<Result> AuthorizeReadAsync<TResource>(ClaimsPrincipal user, TResource resource, CancellationToken cancellationToken = default)
    {
        if (serviceProvider is not null)
        {
            var result = await AuthorizationRuleInvoker.TryInvokeReadAsync(serviceProvider, resource!, cancellationToken);
            if (result is not null)
            {
                return result;
            }
        }

        var authorizationResult = await authorizationService.AuthorizeAsync(user, resource, AuthorizationPolicies.ResourceRead);
        return authorizationResult.Succeeded
            ? Result.Success()
            : Result.Forbidden("Você não tem permissão para acessar este recurso.");
    }

    public async Task<Result> AuthorizeWriteAsync<TResource>(ClaimsPrincipal user, TResource resource, CancellationToken cancellationToken = default)
    {
        if (serviceProvider is not null)
        {
            var result = await AuthorizationRuleInvoker.TryInvokeWriteAsync(serviceProvider, resource!, cancellationToken);
            if (result is not null)
            {
                return result;
            }
        }

        var authorizationResult = await authorizationService.AuthorizeAsync(user, resource, AuthorizationPolicies.ResourceWrite);
        return authorizationResult.Succeeded
            ? Result.Success()
            : Result.Forbidden("Você não tem permissão para realizar esta ação.");
    }

    public async Task<bool> CanReadAsync<TResource>(ClaimsPrincipal user, TResource resource, CancellationToken cancellationToken = default)
    {
        var result = await AuthorizeReadAsync(user, resource, cancellationToken);
        return result.IsSuccess;
    }

    public async Task<bool> CanWriteAsync<TResource>(ClaimsPrincipal user, TResource resource, CancellationToken cancellationToken = default)
    {
        var result = await AuthorizeWriteAsync(user, resource, cancellationToken);
        return result.IsSuccess;
    }
}
