using Bud.Infrastructure.Persistence;
using Bud.Infrastructure.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Bud.Infrastructure.UnitTests.Infrastructure.Services;

public class NotificationRecipientResolverTests
{
    private readonly TestTenantProvider _tenantProvider = new() { IsGlobalAdmin = true };

    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options, _tenantProvider);
    }

    private static async Task<(Organization org, Team team, Employee c1, Employee c2, Employee leader)> CreateTestHierarchy(ApplicationDbContext context)
    {
        var org = new Organization { Id = Guid.NewGuid(), Name = "Test Org" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Test Team", OrganizationId = org.Id, LeaderId = Guid.NewGuid() };
        var leader = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Leader",
            Email = $"leader-{Guid.NewGuid()}@example.com",
            OrganizationId = org.Id,
            Role = EmployeeRole.Leader
        };
        var c1 = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Collab 1",
            Email = $"c1-{Guid.NewGuid()}@example.com",
            OrganizationId = org.Id,
            LeaderId = leader.Id
        };
        var c2 = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Collab 2",
            Email = $"c2-{Guid.NewGuid()}@example.com",
            OrganizationId = org.Id
        };

        context.Organizations.Add(org);
        context.Teams.Add(team);
        context.Employees.AddRange(leader, c1, c2);
        context.EmployeeTeams.AddRange(
            new EmployeeTeam { EmployeeId = c1.Id, TeamId = team.Id },
            new EmployeeTeam { EmployeeId = c2.Id, TeamId = team.Id }
        );
        await context.SaveChangesAsync();

        return (org, team, c1, c2, leader);
    }

    [Fact]
    public async Task ResolveMissionRecipients_EmployeeScope_ReturnsEmployeeAndLeader()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var resolver = new NotificationRecipientResolver(context);
        var (org, _, c1, _, leader) = await CreateTestHierarchy(context);

        var mission = new Mission
        {
            Id = Guid.NewGuid(),
            Name = "Employee Mission",
            OrganizationId = org.Id,
            EmployeeId = c1.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30)
        };
        context.Missions.Add(mission);
        await context.SaveChangesAsync();

        // Act
        var recipients = await resolver.ResolveMissionRecipientsAsync(mission.Id, org.Id);

        // Assert
        recipients.Should().Contain(c1.Id);
        recipients.Should().Contain(leader.Id);
    }

    [Fact]
    public async Task ResolveMissionRecipients_OrganizationScope_ReturnsAllOrgEmployees()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var resolver = new NotificationRecipientResolver(context);
        var (org, _, c1, c2, leader) = await CreateTestHierarchy(context);

        var mission = new Mission
        {
            Id = Guid.NewGuid(),
            Name = "Org Mission",
            OrganizationId = org.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30)
        };
        context.Missions.Add(mission);
        await context.SaveChangesAsync();

        // Act
        var recipients = await resolver.ResolveMissionRecipientsAsync(mission.Id, org.Id);

        // Assert
        recipients.Should().Contain(c1.Id);
        recipients.Should().Contain(c2.Id);
        recipients.Should().Contain(leader.Id);
    }

    [Fact]
    public async Task ResolveMissionRecipients_ExcludesSpecifiedEmployee()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var resolver = new NotificationRecipientResolver(context);
        var (org, _, c1, c2, leader) = await CreateTestHierarchy(context);

        var mission = new Mission
        {
            Id = Guid.NewGuid(),
            Name = "Org Mission",
            OrganizationId = org.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30)
        };
        context.Missions.Add(mission);
        await context.SaveChangesAsync();

        // Act
        var recipients = await resolver.ResolveMissionRecipientsAsync(mission.Id, org.Id, excludeEmployeeId: c1.Id);

        // Assert
        recipients.Should().NotContain(c1.Id);
        recipients.Should().Contain(c2.Id);
        recipients.Should().Contain(leader.Id);
    }

    [Fact]
    public async Task ResolveMissionRecipients_MissionNotFound_ReturnsEmpty()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var resolver = new NotificationRecipientResolver(context);

        // Act
        var recipients = await resolver.ResolveMissionRecipientsAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        recipients.Should().BeEmpty();
    }

    [Fact]
    public async Task ResolveMissionIdFromMetric_ExistingMetric_ReturnsMissionId()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var resolver = new NotificationRecipientResolver(context);
        var (org, _, _, _, _) = await CreateTestHierarchy(context);

        var mission = new Mission
        {
            Id = Guid.NewGuid(),
            Name = "Test Mission",
            OrganizationId = org.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30)
        };
        context.Missions.Add(mission);

        var metric = new Indicator
        {
            Id = Guid.NewGuid(),
            MissionId = mission.Id,
            OrganizationId = org.Id,
            Name = "Test Metric",
            Type = IndicatorType.Quantitative
        };
        context.Indicators.Add(metric);
        await context.SaveChangesAsync();

        // Act
        var result = await resolver.ResolveMissionIdFromIndicatorAsync(metric.Id);

        // Assert
        result.Should().Be(mission.Id);
    }

    [Fact]
    public async Task ResolveMissionIdFromMetric_NonExistentMetric_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var resolver = new NotificationRecipientResolver(context);

        // Act
        var result = await resolver.ResolveMissionIdFromIndicatorAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ResolveEmployeeName_ExistingEmployee_ReturnsFullName()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var resolver = new NotificationRecipientResolver(context);
        var (_, _, c1, _, _) = await CreateTestHierarchy(context);

        // Act
        var name = await resolver.ResolveEmployeeNameAsync(c1.Id);

        // Assert
        name.Should().Be("Collab 1");
    }

    [Fact]
    public async Task ResolveEmployeeName_NonExistentEmployee_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var resolver = new NotificationRecipientResolver(context);

        // Act
        var name = await resolver.ResolveEmployeeNameAsync(Guid.NewGuid());

        // Assert
        name.Should().BeNull();
    }
}
