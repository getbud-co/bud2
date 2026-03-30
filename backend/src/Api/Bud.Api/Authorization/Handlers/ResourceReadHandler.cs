using Bud.Api.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace Bud.Api.Authorization.Handlers;

public sealed class ResourceReadHandler(IServiceProvider serviceProvider)
    : AuthorizationHandler<ResourceReadRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourceReadRequirement requirement)
    {
        if (context.Resource is null)
        {
            return;
        }

        var result = await AuthorizationRuleInvoker.TryInvokeReadAsync(serviceProvider, context.Resource, CancellationToken.None);
        if (result?.IsSuccess == true)
        {
            context.Succeed(requirement);
        }
    }
}
