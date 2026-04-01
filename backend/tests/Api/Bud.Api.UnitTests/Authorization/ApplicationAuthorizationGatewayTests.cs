using System.Security.Claims;
using Bud.Api.Authorization;
using Bud.Application.Common;
using Bud.Application.Features.Employees;
using Bud.Application.Features.Me;
using Bud.Application.Features.Notifications;
using Bud.Application.Ports;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bud.Api.UnitTests.Authorization;

public sealed class ApplicationAuthorizationGatewayTests
{
    private static readonly ClaimsPrincipal User = new(new ClaimsIdentity());

    [Fact]
    public async Task CanReadAsync_WhenPolicySucceeds_ReturnsTrue()
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(s => s.AuthorizeAsync(User, It.IsAny<EmployeeResource>(), AuthorizationPolicies.ResourceRead))
            .ReturnsAsync(AuthorizationResult.Success());

        var gateway = new ApplicationAuthorizationGateway(authorizationService.Object);

        var result = await gateway.CanReadAsync(User, new EmployeeResource(Guid.NewGuid()));

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanReadAsync_WhenPolicyFails_ReturnsFalse()
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(s => s.AuthorizeAsync(User, It.IsAny<DashboardResource>(), AuthorizationPolicies.ResourceRead))
            .ReturnsAsync(AuthorizationResult.Failed());

        var gateway = new ApplicationAuthorizationGateway(authorizationService.Object);

        var result = await gateway.CanReadAsync(User, DashboardResource.Instance);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanWriteAsync_WhenPolicySucceeds_ReturnsTrue()
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(s => s.AuthorizeAsync(User, It.IsAny<NotificationResource>(), AuthorizationPolicies.ResourceWrite))
            .ReturnsAsync(AuthorizationResult.Success());

        var gateway = new ApplicationAuthorizationGateway(authorizationService.Object);

        var result = await gateway.CanWriteAsync(User, new NotificationResource(Guid.NewGuid()));

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanWriteAsync_WhenPolicyFails_ReturnsFalse()
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(s => s.AuthorizeAsync(User, It.IsAny<CreateEmployeeContext>(), AuthorizationPolicies.ResourceWrite))
            .ReturnsAsync(AuthorizationResult.Failed());

        var gateway = new ApplicationAuthorizationGateway(authorizationService.Object);

        var result = await gateway.CanWriteAsync(User, new CreateEmployeeContext(Guid.NewGuid()));

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AuthorizeWriteAsync_WhenRuleExists_ReturnsRuleResult()
    {
        var authorizationService = new Mock<IAuthorizationService>(MockBehavior.Strict);
        var services = new ServiceCollection();
        services.AddSingleton<IWriteAuthorizationRule<CreateEmployeeContext>>(new FakeCreateEmployeeRule(Result.Forbidden("Falha específica.")));
        var serviceProvider = services.BuildServiceProvider();

        var gateway = new ApplicationAuthorizationGateway(authorizationService.Object, serviceProvider);

        var result = await gateway.AuthorizeWriteAsync(User, new CreateEmployeeContext(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        result.Error.Should().Be("Falha específica.");
    }

    private sealed class FakeCreateEmployeeRule(Result result) : IWriteAuthorizationRule<CreateEmployeeContext>
    {
        public Task<Result> EvaluateAsync(CreateEmployeeContext resource, CancellationToken cancellationToken = default)
            => Task.FromResult(result);
    }
}
