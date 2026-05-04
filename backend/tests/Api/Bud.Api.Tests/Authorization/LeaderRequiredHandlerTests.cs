using System.Security.Claims;
using Bud.Api.Authorization.Handlers;
using Bud.Api.Authorization.Requirements;
using Bud.Api.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace Bud.Api.Tests.Authorization;

public sealed class LeaderRequiredHandlerTests
{
    [Fact]
    public async Task Handle_WhenGlobalAdmin_ShouldSucceed()
    {
        var tenantProvider = new TestTenantProvider { IsGlobalAdmin = true };
        var handler = new LeaderRequiredHandler(tenantProvider);
        var requirement = new LeaderRequiredRequirement();
        var context = new AuthorizationHandlerContext([requirement], new ClaimsPrincipal(new ClaimsIdentity()), null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenLeaderRoleClaimExists_ShouldSucceed()
    {
        var tenantProvider = new TestTenantProvider();
        var handler = new LeaderRequiredHandler(tenantProvider);
        var requirement = new LeaderRequiredRequirement();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, nameof(EmployeeRole.Leader))], "test"));
        var context = new AuthorizationHandlerContext([requirement], principal, null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenUserIsNotLeaderNorGlobalAdmin_ShouldFail()
    {
        var tenantProvider = new TestTenantProvider();
        var handler = new LeaderRequiredHandler(tenantProvider);
        var requirement = new LeaderRequiredRequirement();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, nameof(EmployeeRole.IndividualContributor))], "test"));
        var context = new AuthorizationHandlerContext([requirement], principal, null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }
}
