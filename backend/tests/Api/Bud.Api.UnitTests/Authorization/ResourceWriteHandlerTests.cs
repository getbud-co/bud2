using System.Security.Claims;
using Bud.Api.Authorization.Handlers;
using Bud.Api.Authorization.Requirements;
using Bud.Application.Common;
using Bud.Application.Features.Teams;
using Bud.Application.Ports;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bud.Api.UnitTests.Authorization;

public sealed class ResourceWriteHandlerTests
{
    [Fact]
    public async Task HandleRequirementAsync_WhenRuleAllows_Succeeds()
    {
        var services = new ServiceCollection()
            .AddSingleton<IWriteAuthorizationRule<CreateTeamContext>>(new AllowCreateTeamRule())
            .BuildServiceProvider();

        var handler = new ResourceWriteHandler(services);
        var requirement = new ResourceWriteRequirement();
        var context = new AuthorizationHandlerContext(
            [requirement],
            new ClaimsPrincipal(new ClaimsIdentity()),
            new CreateTeamContext(Guid.NewGuid()));

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenRuleDenies_DoesNotSucceed()
    {
        var services = new ServiceCollection()
            .AddSingleton<IWriteAuthorizationRule<CreateTeamContext>>(new DenyCreateTeamRule())
            .BuildServiceProvider();

        var handler = new ResourceWriteHandler(services);
        var requirement = new ResourceWriteRequirement();
        var context = new AuthorizationHandlerContext(
            [requirement],
            new ClaimsPrincipal(new ClaimsIdentity()),
            new CreateTeamContext(Guid.NewGuid()));

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    private sealed class AllowCreateTeamRule : IWriteAuthorizationRule<CreateTeamContext>
    {
        public Task<Result> EvaluateAsync(CreateTeamContext resource, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());
    }

    private sealed class DenyCreateTeamRule : IWriteAuthorizationRule<CreateTeamContext>
    {
        public Task<Result> EvaluateAsync(CreateTeamContext resource, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Forbidden("negado"));
    }
}
