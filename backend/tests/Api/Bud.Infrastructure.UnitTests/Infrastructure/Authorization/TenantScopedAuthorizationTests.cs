using Bud.Application.Common;
using Bud.Infrastructure.Authorization;
using Bud.Infrastructure.UnitTests.Helpers;
using FluentAssertions;
using Xunit;

namespace Bud.Infrastructure.UnitTests.Infrastructure.Authorization;

public sealed class TenantScopedAuthorizationTests
{
    [Fact]
    public async Task AuthorizeReadAsync_WhenTenantMatches_ReturnsSuccess()
    {
        var tenantId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider { TenantId = tenantId };

        var result = await TenantScopedAuthorization.AuthorizeReadAsync(
            tenantProvider,
            _ => Task.FromResult<TestResource?>(new TestResource(Guid.NewGuid(), tenantId)),
            resource => resource.OrganizationId,
            "não encontrado",
            "negado");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AuthorizeWriteAsync_WhenEmployeeMissing_ReturnsForbidden()
    {
        var tenantProvider = new TestTenantProvider { EmployeeId = null };

        var result = await TenantScopedAuthorization.AuthorizeWriteAsync(
            tenantProvider,
            _ => Task.FromResult<TestResource?>(new TestResource(Guid.NewGuid(), Guid.NewGuid())),
            resource => resource.OrganizationId,
            "não encontrado",
            "Funcionário não identificado.",
            "negado");

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        result.Error.Should().Be("Funcionário não identificado.");
    }

    [Fact]
    public async Task AuthorizeWriteAsync_WhenCreateContextTenantDiffers_ReturnsForbidden()
    {
        var tenantProvider = new TestTenantProvider
        {
            TenantId = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid()
        };

        var result = await TenantScopedAuthorization.AuthorizeWriteAsync(
            tenantProvider,
            Guid.NewGuid(),
            "Funcionário não identificado.",
            "negado");

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        result.Error.Should().Be("negado");
    }

    private sealed record TestResource(Guid Id, Guid OrganizationId);
}
