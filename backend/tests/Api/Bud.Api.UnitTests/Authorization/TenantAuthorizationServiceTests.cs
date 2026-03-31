using Bud.Api.Authorization;
using Bud.Infrastructure.Persistence;
using Bud.Api.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Bud.Api.UnitTests.Authorization;

public class TenantAuthorizationServiceTests
{
    private static ApplicationDbContext CreateInMemoryContext(TestTenantProvider tenantProvider)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options, tenantProvider);
    }

    #region UserBelongsToTenantAsync Tests

    [Fact]
    public async Task UserBelongsToTenantAsync_WhenGlobalAdminAndOrganizationExists_ReturnsTrue()
    {
        // Arrange
        var tenantProvider = new TestTenantProvider { IsGlobalAdmin = true };
        using var context = CreateInMemoryContext(tenantProvider);
        var service = new TenantAuthorizationService(context, tenantProvider);

        var tenantId = Guid.NewGuid();
        context.Organizations.Add(new Organization { Id = tenantId, Name = "Org Global Admin" });
        await context.SaveChangesAsync();

        // Act
        var result = await service.UserBelongsToTenantAsync(tenantId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserBelongsToTenantAsync_WhenGlobalAdminAndOrganizationDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var tenantProvider = new TestTenantProvider { IsGlobalAdmin = true };
        using var context = CreateInMemoryContext(tenantProvider);
        var service = new TenantAuthorizationService(context, tenantProvider);

        // Act
        var result = await service.UserBelongsToTenantAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserBelongsToTenantAsync_WhenNoUserEmail_ReturnsFalse()
    {
        // Arrange
        var tenantProvider = new TestTenantProvider { IsGlobalAdmin = false, UserEmail = null };
        using var context = CreateInMemoryContext(tenantProvider);
        var service = new TenantAuthorizationService(context, tenantProvider);

        var tenantId = Guid.NewGuid();

        // Act
        var result = await service.UserBelongsToTenantAsync(tenantId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserBelongsToTenantAsync_WhenEmptyUserEmail_ReturnsFalse()
    {
        // Arrange
        var tenantProvider = new TestTenantProvider { IsGlobalAdmin = false, UserEmail = "" };
        using var context = CreateInMemoryContext(tenantProvider);
        var service = new TenantAuthorizationService(context, tenantProvider);

        var tenantId = Guid.NewGuid();

        // Act
        var result = await service.UserBelongsToTenantAsync(tenantId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserBelongsToTenantAsync_WhenUserIsEmployee_ReturnsTrue()
    {
        // Arrange
        var userEmail = "employee@example.com";
        var tenantProvider = new TestTenantProvider { IsGlobalAdmin = true, UserEmail = userEmail };
        using var context = CreateInMemoryContext(tenantProvider);

        // Create org with employee
        var org = new Organization { Id = Guid.NewGuid(), Name = "Test Org" };
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Employee",
            Email = userEmail,
            Role = EmployeeRole.IndividualContributor,
            OrganizationId = org.Id
        };

        context.Organizations.Add(org);
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        // Now create service with non-admin tenant provider
        var regularTenantProvider = new TestTenantProvider { IsGlobalAdmin = false, UserEmail = userEmail };
        var service = new TenantAuthorizationService(context, regularTenantProvider);

        // Act
        var result = await service.UserBelongsToTenantAsync(org.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserBelongsToTenantAsync_WhenUserNotInOrganization_ReturnsFalse()
    {
        // Arrange
        var userEmail = "other@example.com";
        var tenantProvider = new TestTenantProvider { IsGlobalAdmin = true };
        using var context = CreateInMemoryContext(tenantProvider);

        // Create org without the user
        var org = new Organization { Id = Guid.NewGuid(), Name = "Test Org" };
        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        // Now create service with non-admin tenant provider
        var regularTenantProvider = new TestTenantProvider { IsGlobalAdmin = false, UserEmail = userEmail };
        var service = new TenantAuthorizationService(context, regularTenantProvider);

        // Act
        var result = await service.UserBelongsToTenantAsync(org.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetUserTenantIdsAsync Tests

    [Fact]
    public async Task GetUserTenantIdsAsync_WhenGlobalAdmin_ReturnsAllOrganizations()
    {
        // Arrange
        var tenantProvider = new TestTenantProvider { IsGlobalAdmin = true };
        using var context = CreateInMemoryContext(tenantProvider);
        var service = new TenantAuthorizationService(context, tenantProvider);

        var org1 = new Organization { Id = Guid.NewGuid(), Name = "Org 1" };
        var org2 = new Organization { Id = Guid.NewGuid(), Name = "Org 2" };
        var org3 = new Organization { Id = Guid.NewGuid(), Name = "Org 3" };
        context.Organizations.AddRange(org1, org2, org3);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetUserTenantIdsAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain([org1.Id, org2.Id, org3.Id]);
    }

    [Fact]
    public async Task GetUserTenantIdsAsync_WhenNoUserEmail_ReturnsEmptyList()
    {
        // Arrange
        var tenantProvider = new TestTenantProvider { IsGlobalAdmin = false, UserEmail = null };
        using var context = CreateInMemoryContext(tenantProvider);
        var service = new TenantAuthorizationService(context, tenantProvider);

        var org = new Organization { Id = Guid.NewGuid(), Name = "Org" };
        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetUserTenantIdsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserTenantIdsAsync_WhenEmptyUserEmail_ReturnsEmptyList()
    {
        // Arrange
        var tenantProvider = new TestTenantProvider { IsGlobalAdmin = false, UserEmail = "" };
        using var context = CreateInMemoryContext(tenantProvider);
        var service = new TenantAuthorizationService(context, tenantProvider);

        var org = new Organization { Id = Guid.NewGuid(), Name = "Org" };
        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetUserTenantIdsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserTenantIdsAsync_ReturnsOwnedAndEmployeeOrganizations()
    {
        // Arrange
        var userEmail = "user@example.com";
        var setupTenantProvider = new TestTenantProvider { IsGlobalAdmin = true };
        using var context = CreateInMemoryContext(setupTenantProvider);

        // Create organizations
        var ownedOrg = new Organization { Id = Guid.NewGuid(), Name = "Owned Org" };
        var memberOrg = new Organization { Id = Guid.NewGuid(), Name = "Member Org" };
        var otherOrg = new Organization { Id = Guid.NewGuid(), Name = "Other Org" };
        context.Organizations.AddRange(ownedOrg, memberOrg, otherOrg);

        // Create workspace and teams
        var ownedTeam = new Team { Id = Guid.NewGuid(), Name = "Owned Team", OrganizationId = ownedOrg.Id, LeaderId = Guid.NewGuid() };
        var memberTeam = new Team { Id = Guid.NewGuid(), Name = "Member Team", OrganizationId = memberOrg.Id, LeaderId = Guid.NewGuid() };
        context.Teams.AddRange(ownedTeam, memberTeam);

        // Create employee in ownedOrg
        var owner = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "User",
            Email = userEmail,
            Role = EmployeeRole.IndividualContributor,
            OrganizationId = ownedOrg.Id,
            TeamId = ownedTeam.Id
        };
        context.Employees.Add(owner);

        // Create member employee in memberOrg
        var member = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "User Member",
            Email = userEmail,
            Role = EmployeeRole.IndividualContributor,
            OrganizationId = memberOrg.Id,
            TeamId = memberTeam.Id
        };
        context.Employees.Add(member);

        await context.SaveChangesAsync();

        // Now create service with regular user tenant provider
        var regularTenantProvider = new TestTenantProvider { IsGlobalAdmin = false, UserEmail = userEmail };
        var service = new TenantAuthorizationService(context, regularTenantProvider);

        // Act
        var result = await service.GetUserTenantIdsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain([ownedOrg.Id, memberOrg.Id]);
        result.Should().NotContain(otherOrg.Id);
    }

    [Fact]
    public async Task GetUserTenantIdsAsync_DeduplicatesOrganizations()
    {
        // Arrange: User is both leader and employee of the same org
        var userEmail = "user@example.com";
        var setupTenantProvider = new TestTenantProvider { IsGlobalAdmin = true };
        using var context = CreateInMemoryContext(setupTenantProvider);

        var org = new Organization { Id = Guid.NewGuid(), Name = "Same Org" };
        context.Organizations.Add(org);

        var team = new Team { Id = Guid.NewGuid(), Name = "Team", OrganizationId = org.Id, LeaderId = Guid.NewGuid() };
        context.Teams.Add(team);

        // User belongs to the same org only once
        var ownerEmployee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "User",
            Email = userEmail,
            Role = EmployeeRole.Leader,
            OrganizationId = org.Id,
            TeamId = team.Id
        };
        context.Employees.Add(ownerEmployee);

        await context.SaveChangesAsync();

        var regularTenantProvider = new TestTenantProvider { IsGlobalAdmin = false, UserEmail = userEmail };
        var service = new TenantAuthorizationService(context, regularTenantProvider);

        // Act
        var result = await service.GetUserTenantIdsAsync();

        // Assert: Should return the org only once
        result.Should().ContainSingle();
        result.Should().Contain(org.Id);
    }

    #endregion
}
