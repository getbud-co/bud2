using Bud.Api.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace Bud.Api.Authorization.Handlers;

public sealed class ResourceWriteHandler(IServiceProvider serviceProvider)
    : AuthorizationHandler<ResourceWriteRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourceWriteRequirement requirement)
    {
        if (context.Resource is null)
        {
            return;
        }

        var result = await AuthorizationRuleInvoker.TryInvokeWriteAsync(serviceProvider, context.Resource, CancellationToken.None);
        if (result?.IsSuccess == true)
        {
            context.Succeed(requirement);
        }
    }
}
