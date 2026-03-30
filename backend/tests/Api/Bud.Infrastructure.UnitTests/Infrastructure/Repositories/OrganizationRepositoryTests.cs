using Bud.Infrastructure.Persistence;
using Bud.Infrastructure.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Bud.Infrastructure.UnitTests.Infrastructure.Repositories;

public sealed class OrganizationRepositoryTests
{
    private readonly TestTenantProvider _tenantProvider = new() { IsGlobalAdmin = true };

    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options, _tenantProvider);
    }

    private static Organization CreateTestOrganization(string name = "Test Org")
    {
        return new Organization { Id = Guid.NewGuid(), Name = name };
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenOrganizationExists_ReturnsOrganization()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new OrganizationRepository(context);

        var org = CreateTestOrganization();
        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(org.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(org.Id);
        result.Name.Should().Be("Test Org");
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrganizationNotFound_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new OrganizationRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedResults()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new OrganizationRepository(context);

        for (int i = 0; i < 5; i++)
        {
            context.Organizations.Add(CreateTestOrganization($"Org {i:D2}"));
        }
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync(null, 1, 2);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_WithCaseInsensitiveSearch_FiltersCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new OrganizationRepository(context);

        context.Organizations.AddRange(
            CreateTestOrganization("ALPHA Corp"),
            CreateTestOrganization("Beta Inc"));
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync("alpha", 1, 10);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("ALPHA Corp");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOrderedByName()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new OrganizationRepository(context);

        context.Organizations.AddRange(
            CreateTestOrganization("Zebra"),
            CreateTestOrganization("Alpha"),
            CreateTestOrganization("Mango"));
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync(null, 1, 10);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items[0].Name.Should().Be("Alpha");
        result.Items[1].Name.Should().Be("Mango");
        result.Items[2].Name.Should().Be("Zebra");
    }

    #endregion

    #region GetEmployeesAsync Tests

    [Fact]
    public async Task GetEmployeesAsync_ReturnsEmployeesForOrganization()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new OrganizationRepository(context);

        var org = CreateTestOrganization();
        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        context.Employees.AddRange(
            new Employee { Id = Guid.NewGuid(), FullName = "Alice", Email = "alice@test.com", OrganizationId = org.Id },
            new Employee { Id = Guid.NewGuid(), FullName = "Bob", Email = "bob@test.com", OrganizationId = org.Id });
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetEmployeesAsync(org.Id, 1, 10);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(2);
    }

    [Fact]
    public async Task GetEmployeesAsync_ReturnsPaginatedResults()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new OrganizationRepository(context);

        var org = CreateTestOrganization();
        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        for (int i = 0; i < 5; i++)
        {
            context.Employees.Add(new Employee
            {
                Id = Guid.NewGuid(),
                FullName = $"Employee {i:D2}",
                Email = $"collab{i}@test.com",
                OrganizationId = org.Id
            });
        }
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetEmployeesAsync(org.Id, 1, 2);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(2);
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_WhenOrganizationExists_ReturnsTrue()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new OrganizationRepository(context);

        var org = CreateTestOrganization();
        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.ExistsAsync(org.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenOrganizationNotFound_ReturnsFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new OrganizationRepository(context);

        // Act
        var result = await repository.ExistsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region HasEmployeesAsync Tests

    [Fact]
    public async Task HasEmployeesAsync_WhenHasEmployees_ReturnsTrue()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new OrganizationRepository(context);

        var org = CreateTestOrganization();
        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        context.Employees.Add(new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Test",
            Email = "test@test.com",
            OrganizationId = org.Id
        });
        await context.SaveChangesAsync();

        // Act
        var result = await repository.HasEmployeesAsync(org.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasEmployeesAsync_WhenNoEmployees_ReturnsFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new OrganizationRepository(context);

        var org = CreateTestOrganization();
        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.HasEmployeesAsync(org.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region AddAsync / RemoveAsync / SaveChangesAsync Tests

    [Fact]
    public async Task AddAsync_PersistsOrganization()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new OrganizationRepository(context);

        var org = CreateTestOrganization("New Org");

        // Act
        await repository.AddAsync(org);
        await repository.SaveChangesAsync();

        // Assert
        var persisted = await context.Organizations.FindAsync(org.Id);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("New Org");
    }

    [Fact]
    public async Task RemoveAsync_DeletesOrganization()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new OrganizationRepository(context);

        var org = CreateTestOrganization("To Delete");
        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        // Re-fetch tracked entity
        var tracked = await context.Organizations.FirstAsync(o => o.Id == org.Id);

        // Act
        await repository.RemoveAsync(tracked);
        await repository.SaveChangesAsync();

        // Assert
        var persisted = await context.Organizations.FindAsync(org.Id);
        persisted.Should().BeNull();
    }

    #endregion
}
