using System.Security.Claims;
using Bud.Api.Authorization.Handlers;
using Bud.Api.Authorization.Requirements;
using Bud.Application.Common;
using Bud.Application.Features.Employees;
using Bud.Application.Ports;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bud.Api.UnitTests.Authorization;

public sealed class ResourceReadHandlerTests
{
    [Fact]
    public async Task HandleRequirementAsync_WhenRuleAllows_Succeeds()
    {
        var services = new ServiceCollection()
            .AddSingleton<IReadAuthorizationRule<EmployeeResource>>(new AllowEmployeeReadRule())
            .BuildServiceProvider();

        var handler = new ResourceReadHandler(services);
        var requirement = new ResourceReadRequirement();
        var context = new AuthorizationHandlerContext(
            [requirement],
            new ClaimsPrincipal(new ClaimsIdentity()),
            new EmployeeResource(Guid.NewGuid()));

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenRuleDenies_DoesNotSucceed()
    {
        var services = new ServiceCollection()
            .AddSingleton<IReadAuthorizationRule<EmployeeResource>>(new DenyEmployeeReadRule())
            .BuildServiceProvider();

        var handler = new ResourceReadHandler(services);
        var requirement = new ResourceReadRequirement();
        var context = new AuthorizationHandlerContext(
            [requirement],
            new ClaimsPrincipal(new ClaimsIdentity()),
            new EmployeeResource(Guid.NewGuid()));

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    private sealed class AllowEmployeeReadRule : IReadAuthorizationRule<EmployeeResource>
    {
        public Task<Result> EvaluateAsync(EmployeeResource resource, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());
    }

    private sealed class DenyEmployeeReadRule : IReadAuthorizationRule<EmployeeResource>
    {
        public Task<Result> EvaluateAsync(EmployeeResource resource, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Forbidden("negado"));
    }
}
