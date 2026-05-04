using Bud.Infrastructure.Persistence;
using Bud.Api.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Bud.Api.Tests.MultiTenancy;

public class ApplicationDbContextTenantFilterTests
{
    private static ApplicationDbContext CreateContext(TestTenantProvider tenantProvider)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options, tenantProvider);
    }

    [Fact]
    public async Task QueryFilters_WithNullTenantAndNonAdmin_ReturnsNoOrganizations()
    {
        // Arrange
        var tenantProvider = new TestTenantProvider { TenantId = null, IsGlobalAdmin = false };
        using var context = CreateContext(tenantProvider);

        var org1 = Organization.Create(Guid.NewGuid(), OrganizationDomainName.Create("org1.com"));
        var org2 = Organization.Create(Guid.NewGuid(), OrganizationDomainName.Create("org2.com"));
        context.Organizations.AddRange(org1, org2);
        await context.SaveChangesAsync();

        // Act
        var organizations = await context.Organizations.ToListAsync();

        // Assert
        organizations.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryFilters_WithNullTenantAndGlobalAdmin_ReturnsAllOrganizations()
    {
        // Arrange
        var tenantProvider = new TestTenantProvider { TenantId = null, IsGlobalAdmin = true };
        using var context = CreateContext(tenantProvider);

        var org1 = Organization.Create(Guid.NewGuid(), OrganizationDomainName.Create("org1.com"));
        var org2 = Organization.Create(Guid.NewGuid(), OrganizationDomainName.Create("org2.com"));
        context.Organizations.AddRange(org1, org2);
        await context.SaveChangesAsync();

        // Act
        var organizations = await context.Organizations.ToListAsync();

        // Assert
        organizations.Should().HaveCount(2);
    }

    [Fact]
    public async Task QueryFilters_WithTenantId_ReturnsOnlyTenantOrganizations()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider { TenantId = tenantId, IsGlobalAdmin = false };
        using var context = CreateContext(tenantProvider);

        var org1 = Organization.Create(tenantId, OrganizationDomainName.Create("tenant.org"));
        var org2 = Organization.Create(Guid.NewGuid(), OrganizationDomainName.Create("other.org"));
        context.Organizations.AddRange(org1, org2);
        await context.SaveChangesAsync();

        // Act
        var organizations = await context.Organizations.ToListAsync();

        // Assert
        organizations.Should().ContainSingle(o => o.Id == tenantId);
    }
}
