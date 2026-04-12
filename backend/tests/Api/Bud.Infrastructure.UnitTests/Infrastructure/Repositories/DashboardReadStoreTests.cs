using Bud.Infrastructure.Persistence;
using Bud.Infrastructure.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Bud.Infrastructure.UnitTests.Infrastructure.Repositories;

public class DashboardReadStoreTests
{
    private readonly TestTenantProvider _tenantProvider = new() { IsGlobalAdmin = true };

    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options, _tenantProvider);
    }

    [Fact]
    public async Task GetMyDashboardAsync_EmployeeNotFound_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new DashboardReadStore(context);

        // Act
        var result = await repository.GetMyDashboardAsync(Guid.NewGuid(), null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMyDashboardAsync_EmployeeWithLeader_ReturnsLeaderInfo()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var org = new Organization { Id = Guid.NewGuid(), Name = "Org" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Diretoria de RH", OrganizationId = org.Id };
        var leader = new Employee { Id = Guid.NewGuid(), FullName = "Maria Silva", Email = "maria@test.com" };
        var employee = new Employee { Id = Guid.NewGuid(), FullName = "Joao Santos", Email = "joao@test.com" };

        context.Organizations.Add(org);
        context.Teams.Add(team);
        context.Employees.AddRange(leader, employee);
        context.OrganizationEmployeeMembers.Add(new OrganizationEmployeeMember
        {
            EmployeeId = leader.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.TeamLeader
        });
        context.OrganizationEmployeeMembers.Add(new OrganizationEmployeeMember
        {
            EmployeeId = employee.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.Contributor,
            LeaderId = leader.Id
        });
        context.Set<EmployeeTeam>().Add(new EmployeeTeam { EmployeeId = leader.Id, TeamId = team.Id });
        await context.SaveChangesAsync();

        var repository = new DashboardReadStore(context);

        // Act
        var result = await repository.GetMyDashboardAsync(employee.Id, null);

        // Assert
        result.Should().NotBeNull();
        result!.TeamHealth.Leader.Should().NotBeNull();
        result.TeamHealth.Leader!.FullName.Should().Be("Maria Silva");
        result.TeamHealth.Leader.Initials.Should().Be("MS");
        result.TeamHealth.Leader.TeamName.Should().Be("Diretoria de RH");
    }

    [Fact]
    public async Task GetMyDashboardAsync_EmployeeIsLeader_ReturnsSelfAsLeader()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var org = new Organization { Id = Guid.NewGuid(), Name = "Org" };
        var leader = new Employee { Id = Guid.NewGuid(), FullName = "Maria Silva", Email = "maria@test.com" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Engenharia", OrganizationId = org.Id };
        context.Organizations.Add(org);
        context.Employees.Add(leader);
        context.OrganizationEmployeeMembers.Add(new OrganizationEmployeeMember
        {
            EmployeeId = leader.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.TeamLeader
        });
        context.Teams.Add(team);
        context.Set<EmployeeTeam>().Add(new EmployeeTeam { EmployeeId = leader.Id, TeamId = team.Id });
        await context.SaveChangesAsync();

        var repository = new DashboardReadStore(context);

        // Act
        var result = await repository.GetMyDashboardAsync(leader.Id, null);

        // Assert
        result.Should().NotBeNull();
        result!.TeamHealth.Leader.Should().NotBeNull();
        result.TeamHealth.Leader!.Id.Should().Be(leader.Id);
        result.TeamHealth.Leader.TeamName.Should().Be("Engenharia");
    }

    [Fact]
    public async Task GetMyDashboardAsync_LeaderWithoutTeam_ReturnsEmptyTeamName()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var org = new Organization { Id = Guid.NewGuid(), Name = "Org" };
        var leader = new Employee { Id = Guid.NewGuid(), FullName = "Carlos Souza", Email = "carlos@test.com" };

        context.Organizations.Add(org);
        context.Employees.Add(leader);
        context.OrganizationEmployeeMembers.Add(new OrganizationEmployeeMember
        {
            EmployeeId = leader.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.TeamLeader
        });
        await context.SaveChangesAsync();

        var repository = new DashboardReadStore(context);

        // Act
        var result = await repository.GetMyDashboardAsync(leader.Id, null);

        // Assert
        result.Should().NotBeNull();
        result!.TeamHealth.Leader.Should().NotBeNull();
        result.TeamHealth.Leader!.TeamName.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMyDashboardAsync_EmployeeWithNoLeader_ReturnsNullLeader()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var org = new Organization { Id = Guid.NewGuid(), Name = "Org" };
        var employee = new Employee { Id = Guid.NewGuid(), FullName = "Joao Santos", Email = "joao@test.com" };

        context.Organizations.Add(org);
        context.Employees.Add(employee);
        context.OrganizationEmployeeMembers.Add(new OrganizationEmployeeMember
        {
            EmployeeId = employee.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.Contributor
        });
        await context.SaveChangesAsync();

        var repository = new DashboardReadStore(context);

        // Act
        var result = await repository.GetMyDashboardAsync(employee.Id, null);

        // Assert
        result.Should().NotBeNull();
        result!.TeamHealth.Leader.Should().BeNull();
    }

    [Fact]
    public async Task GetMyDashboardAsync_LeaderWithTeamMembers_ReturnsTeamMembersFromEmployeeTeams()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var org = new Organization { Id = Guid.NewGuid(), Name = "Org" };
        var leader = new Employee { Id = Guid.NewGuid(), FullName = "Maria Silva", Email = "maria@test.com" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Engenharia", OrganizationId = org.Id };
        var member1 = new Employee { Id = Guid.NewGuid(), FullName = "Ana Lima", Email = "ana@test.com" };
        var member2 = new Employee { Id = Guid.NewGuid(), FullName = "Carlos Souza", Email = "carlos@test.com" };

        context.Organizations.Add(org);
        context.Employees.Add(leader);
        context.Teams.Add(team);
        context.Employees.AddRange(member1, member2);
        context.OrganizationEmployeeMembers.AddRange(
            new OrganizationEmployeeMember { EmployeeId = leader.Id, OrganizationId = org.Id, Role = EmployeeRole.TeamLeader },
            new OrganizationEmployeeMember { EmployeeId = member1.Id, OrganizationId = org.Id, Role = EmployeeRole.Contributor },
            new OrganizationEmployeeMember { EmployeeId = member2.Id, OrganizationId = org.Id, Role = EmployeeRole.Contributor }
        );
        context.Set<EmployeeTeam>().AddRange(
            new EmployeeTeam { EmployeeId = leader.Id, TeamId = team.Id },
            new EmployeeTeam { EmployeeId = member1.Id, TeamId = team.Id },
            new EmployeeTeam { EmployeeId = member2.Id, TeamId = team.Id }
        );
        await context.SaveChangesAsync();

        var repository = new DashboardReadStore(context);

        // Act
        var result = await repository.GetMyDashboardAsync(leader.Id, null);

        // Assert
        result.Should().NotBeNull();
        result!.TeamHealth.TeamMembers.Should().HaveCount(2);
        var memberNames = result.TeamHealth.TeamMembers.Select(m => m.FullName).ToList();
        memberNames.Should().Contain("Ana Lima");
        memberNames.Should().Contain("Carlos Souza");
    }

    [Fact]
    public async Task GetMyDashboardAsync_EmployeeWithPrimaryTeam_ReturnsTeamMembersFromEmployeeTeams()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var org = new Organization { Id = Guid.NewGuid(), Name = "Org" };
        var leader = new Employee { Id = Guid.NewGuid(), FullName = "Maria Silva", Email = "maria@test.com" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Engenharia", OrganizationId = org.Id };
        var member1 = new Employee { Id = Guid.NewGuid(), FullName = "Ana Lima", Email = "ana@test.com" };
        var member2 = new Employee { Id = Guid.NewGuid(), FullName = "Carlos Souza", Email = "carlos@test.com" };

        context.Organizations.Add(org);
        context.Employees.Add(leader);
        context.Teams.Add(team);
        context.Employees.AddRange(member1, member2);
        context.OrganizationEmployeeMembers.AddRange(
            new OrganizationEmployeeMember { EmployeeId = leader.Id, OrganizationId = org.Id, Role = EmployeeRole.TeamLeader },
            new OrganizationEmployeeMember { EmployeeId = member1.Id, OrganizationId = org.Id, Role = EmployeeRole.Contributor },
            new OrganizationEmployeeMember { EmployeeId = member2.Id, OrganizationId = org.Id, Role = EmployeeRole.Contributor }
        );
        context.Set<EmployeeTeam>().AddRange(
            new EmployeeTeam { EmployeeId = leader.Id, TeamId = team.Id },
            new EmployeeTeam { EmployeeId = member1.Id, TeamId = team.Id },
            new EmployeeTeam { EmployeeId = member2.Id, TeamId = team.Id }
        );
        await context.SaveChangesAsync();

        var repository = new DashboardReadStore(context);

        // Act — member1 views "Meu time" (no teamId)
        var result = await repository.GetMyDashboardAsync(member1.Id, null);

        // Assert
        result.Should().NotBeNull();
        result!.TeamHealth.TeamMembers.Should().HaveCount(2);
        var memberNames = result.TeamHealth.TeamMembers.Select(m => m.FullName).ToList();
        memberNames.Should().Contain("Ana Lima");
        memberNames.Should().Contain("Carlos Souza");
    }

    [Fact]
    public async Task GetMyDashboardAsync_WeeklyAccess_CalculatesCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var org = new Organization { Id = Guid.NewGuid(), Name = "Org" };
        var leader = new Employee { Id = Guid.NewGuid(), FullName = "Leader Test", Email = "leader@test.com" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Team", OrganizationId = org.Id };
        var member1 = new Employee { Id = Guid.NewGuid(), FullName = "M1 Test", Email = "m1@test.com" };
        var member2 = new Employee { Id = Guid.NewGuid(), FullName = "M2 Test", Email = "m2@test.com" };

        context.Organizations.Add(org);
        context.Employees.Add(leader);
        context.Teams.Add(team);
        context.Employees.AddRange(member1, member2);
        context.OrganizationEmployeeMembers.AddRange(
            new OrganizationEmployeeMember { EmployeeId = leader.Id, OrganizationId = org.Id, Role = EmployeeRole.TeamLeader },
            new OrganizationEmployeeMember { EmployeeId = member1.Id, OrganizationId = org.Id, Role = EmployeeRole.Contributor },
            new OrganizationEmployeeMember { EmployeeId = member2.Id, OrganizationId = org.Id, Role = EmployeeRole.Contributor }
        );
        context.Set<EmployeeTeam>().AddRange(
            new EmployeeTeam { EmployeeId = leader.Id, TeamId = team.Id },
            new EmployeeTeam { EmployeeId = member1.Id, TeamId = team.Id },
            new EmployeeTeam { EmployeeId = member2.Id, TeamId = team.Id }
        );

        // Member 1 accessed this week
        context.EmployeeAccessLogs.Add(new EmployeeAccessLog
        {
            Id = Guid.NewGuid(),
            EmployeeId = member1.Id,
            OrganizationId = org.Id,
            AccessedAt = DateTime.UtcNow.AddDays(-1)
        });

        await context.SaveChangesAsync();

        var repository = new DashboardReadStore(context);

        // Act
        var result = await repository.GetMyDashboardAsync(leader.Id, null);

        // Assert
        result.Should().NotBeNull();
        // 1 out of 3 (leader + 2 members) accessed = 33%
        result!.TeamHealth.WeeklyAccess.Percentage.Should().Be(33);
    }

    [Fact]
    public async Task GetMyDashboardAsync_EngagementScore_HighLevel()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var org = new Organization { Id = Guid.NewGuid(), Name = "Org" };
        var leader = new Employee { Id = Guid.NewGuid(), FullName = "Leader Test", Email = "leader@test.com" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Team", OrganizationId = org.Id };
        var member = new Employee { Id = Guid.NewGuid(), FullName = "Member One", Email = "member1@test.com" };

        context.Organizations.Add(org);
        context.Employees.Add(leader);
        context.Teams.Add(team);
        context.Employees.Add(member);
        context.OrganizationEmployeeMembers.AddRange(
            new OrganizationEmployeeMember { EmployeeId = leader.Id, OrganizationId = org.Id, Role = EmployeeRole.TeamLeader },
            new OrganizationEmployeeMember { EmployeeId = member.Id, OrganizationId = org.Id, Role = EmployeeRole.Contributor }
        );
        context.Set<EmployeeTeam>().Add(
            new EmployeeTeam { EmployeeId = leader.Id, TeamId = team.Id }
        );
        context.Set<EmployeeTeam>().Add(
            new EmployeeTeam { EmployeeId = member.Id, TeamId = team.Id }
        );

        // Access log (100% access)
        context.EmployeeAccessLogs.Add(new EmployeeAccessLog
        {
            Id = Guid.NewGuid(),
            EmployeeId = member.Id,
            OrganizationId = org.Id,
            AccessedAt = DateTime.UtcNow.AddDays(-1)
        });

        // Active mission with recent checkin (100% missions updated)
        var mission = new Mission
        {
            Id = Guid.NewGuid(),
            Name = "M1",
            OrganizationId = org.Id,
            EmployeeId = member.Id,
            Status = MissionStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(30)
        };
        context.Missions.Add(mission);

        var metric = new Indicator
        {
            Id = Guid.NewGuid(),
            Name = "KR1",
            MissionId = mission.Id,
            OrganizationId = org.Id,
            Type = IndicatorType.Quantitative
        };
        context.Indicators.Add(metric);

        // Recent checkin with high confidence
        context.Checkins.Add(new Checkin
        {
            Id = Guid.NewGuid(),
            IndicatorId = metric.Id,
            EmployeeId = member.Id,
            OrganizationId = org.Id,
            CheckinDate = DateTime.UtcNow.AddDays(-1),
            ConfidenceLevel = 5,
            Value = 80
        });

        await context.SaveChangesAsync();

        var repository = new DashboardReadStore(context);

        // Act
        var result = await repository.GetMyDashboardAsync(leader.Id, null);

        // Assert
        result.Should().NotBeNull();
        result!.TeamHealth.Engagement.Score.Should().BeGreaterThanOrEqualTo(70);
        result.TeamHealth.Engagement.Level.Should().Be("high");
    }

    [Fact]
    public async Task GetMyDashboardAsync_NoTeamMembers_ReturnsZeroIndicators()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var org = new Organization { Id = Guid.NewGuid(), Name = "Org" };
        var employee = new Employee { Id = Guid.NewGuid(), FullName = "Alone Person", Email = "alone@test.com" };

        context.Organizations.Add(org);
        context.Employees.Add(employee);
        context.OrganizationEmployeeMembers.Add(new OrganizationEmployeeMember
        {
            EmployeeId = employee.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.Contributor
        });
        await context.SaveChangesAsync();

        var repository = new DashboardReadStore(context);

        // Act
        var result = await repository.GetMyDashboardAsync(employee.Id, null);

        // Assert
        result.Should().NotBeNull();
        result!.TeamHealth.WeeklyAccess.Percentage.Should().Be(0);
        result.TeamHealth.MissionsUpdated.Percentage.Should().Be(0);
        result.TeamHealth.FormsResponded.IsPlaceholder.Should().BeTrue();
    }

    [Fact]
    public async Task GetMyDashboardAsync_PendingTasks_ReturnsOverdueMissions()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var org = new Organization { Id = Guid.NewGuid(), Name = "Org" };
        var employee = new Employee { Id = Guid.NewGuid(), FullName = "Task Person", Email = "task@test.com" };

        var mission = new Mission
        {
            Id = Guid.NewGuid(),
            Name = "Overdue Mission",
            OrganizationId = org.Id,
            EmployeeId = employee.Id,
            Status = MissionStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(30)
        };

        var metric = new Indicator
        {
            Id = Guid.NewGuid(),
            Name = "KR",
            MissionId = mission.Id,
            OrganizationId = org.Id,
            Type = IndicatorType.Quantitative
        };

        // Checkin older than 7 days
        var checkin = new Checkin
        {
            Id = Guid.NewGuid(),
            IndicatorId = metric.Id,
            EmployeeId = employee.Id,
            OrganizationId = org.Id,
            CheckinDate = DateTime.UtcNow.AddDays(-10),
            ConfidenceLevel = 3,
            Value = 50
        };

        context.Organizations.Add(org);
        context.Employees.Add(employee);
        context.OrganizationEmployeeMembers.Add(new OrganizationEmployeeMember
        {
            EmployeeId = employee.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.Contributor
        });
        context.Missions.Add(mission);
        context.Indicators.Add(metric);
        context.Checkins.Add(checkin);
        await context.SaveChangesAsync();

        var repository = new DashboardReadStore(context);

        // Act
        var result = await repository.GetMyDashboardAsync(employee.Id, null);

        // Assert
        result.Should().NotBeNull();
        result!.PendingTasks.Should().HaveCount(1);
        result.PendingTasks[0].Title.Should().Be("Overdue Mission");
        result.PendingTasks[0].TaskType.Should().Be("mission_checkin");
    }

    [Fact]
    public async Task GetMyDashboardAsync_RecentCheckin_NoPendingTask()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var org = new Organization { Id = Guid.NewGuid(), Name = "Org" };
        var employee = new Employee { Id = Guid.NewGuid(), FullName = "Updated Person", Email = "updated@test.com" };

        var mission = new Mission
        {
            Id = Guid.NewGuid(),
            Name = "Up to date Mission",
            OrganizationId = org.Id,
            EmployeeId = employee.Id,
            Status = MissionStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(30)
        };

        var metric = new Indicator
        {
            Id = Guid.NewGuid(),
            Name = "KR",
            MissionId = mission.Id,
            OrganizationId = org.Id,
            Type = IndicatorType.Quantitative
        };

        // Recent checkin (within 7 days)
        var checkin = new Checkin
        {
            Id = Guid.NewGuid(),
            IndicatorId = metric.Id,
            EmployeeId = employee.Id,
            OrganizationId = org.Id,
            CheckinDate = DateTime.UtcNow.AddDays(-2),
            ConfidenceLevel = 4,
            Value = 70
        };

        context.Organizations.Add(org);
        context.Employees.Add(employee);
        context.OrganizationEmployeeMembers.Add(new OrganizationEmployeeMember
        {
            EmployeeId = employee.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.Contributor
        });
        context.Missions.Add(mission);
        context.Indicators.Add(metric);
        context.Checkins.Add(checkin);
        await context.SaveChangesAsync();

        var repository = new DashboardReadStore(context);

        // Act
        var result = await repository.GetMyDashboardAsync(employee.Id, null);

        // Assert
        result.Should().NotBeNull();
        result!.PendingTasks.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMyDashboardAsync_WithTeamId_ReturnsTeamMembersFromEmployeeTeam()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var org = new Organization { Id = Guid.NewGuid(), Name = "Org" };
        var teamLeader = new Employee { Id = Guid.NewGuid(), FullName = "Alpha Leader", Email = "alpha-leader@test.com" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Squad Alpha", OrganizationId = org.Id };
        var employee = new Employee { Id = Guid.NewGuid(), FullName = "Viewer User", Email = "viewer@test.com" };
        var member1 = new Employee { Id = Guid.NewGuid(), FullName = "Alice Team", Email = "alice@test.com" };
        var member2 = new Employee { Id = Guid.NewGuid(), FullName = "Bob Team", Email = "bob@test.com" };
        var outsider = new Employee { Id = Guid.NewGuid(), FullName = "Charlie Outside", Email = "charlie@test.com" };

        context.Organizations.Add(org);
        context.Employees.Add(teamLeader);
        context.Teams.Add(team);
        context.Employees.AddRange(employee, member1, member2, outsider);
        context.OrganizationEmployeeMembers.AddRange(
            new OrganizationEmployeeMember { EmployeeId = teamLeader.Id, OrganizationId = org.Id, Role = EmployeeRole.TeamLeader },
            new OrganizationEmployeeMember { EmployeeId = employee.Id, OrganizationId = org.Id, Role = EmployeeRole.Contributor },
            new OrganizationEmployeeMember { EmployeeId = member1.Id, OrganizationId = org.Id, Role = EmployeeRole.Contributor },
            new OrganizationEmployeeMember { EmployeeId = member2.Id, OrganizationId = org.Id, Role = EmployeeRole.Contributor }
        );
        context.Set<EmployeeTeam>().AddRange(
            new EmployeeTeam { EmployeeId = member1.Id, TeamId = team.Id },
            new EmployeeTeam { EmployeeId = member2.Id, TeamId = team.Id }
        );
        await context.SaveChangesAsync();

        var repository = new DashboardReadStore(context);

        // Act
        var result = await repository.GetMyDashboardAsync(employee.Id, teamId: team.Id);

        // Assert
        result.Should().NotBeNull();
        result!.TeamHealth.TeamMembers.Should().HaveCount(2);
        var names = result.TeamHealth.TeamMembers.Select(m => m.FullName).ToList();
        names.Should().Contain("Alice Team");
        names.Should().Contain("Bob Team");
        names.Should().NotContain("Charlie Outside");
    }

    [Fact]
    public async Task GetMyDashboardAsync_WithTeamId_ReturnsTeamLeader()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var org = new Organization { Id = Guid.NewGuid(), Name = "Org" };
        var teamLeader = new Employee { Id = Guid.NewGuid(), FullName = "Beta Leader", Email = "beta-leader@test.com" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Squad Beta", OrganizationId = org.Id };
        var employee = new Employee { Id = Guid.NewGuid(), FullName = "Viewer User", Email = "viewer@test.com" };

        context.Organizations.Add(org);
        context.Employees.Add(teamLeader);
        context.Teams.Add(team);
        context.Employees.Add(employee);
        context.OrganizationEmployeeMembers.AddRange(
            new OrganizationEmployeeMember { EmployeeId = teamLeader.Id, OrganizationId = org.Id, Role = EmployeeRole.TeamLeader },
            new OrganizationEmployeeMember { EmployeeId = employee.Id, OrganizationId = org.Id, Role = EmployeeRole.Contributor }
        );
        context.Set<EmployeeTeam>().Add(
            new EmployeeTeam { EmployeeId = employee.Id, TeamId = team.Id }
        );
        await context.SaveChangesAsync();

        var repository = new DashboardReadStore(context);

        // Act
        var result = await repository.GetMyDashboardAsync(employee.Id, teamId: team.Id);

        // Assert
        result.Should().NotBeNull();
        result!.TeamHealth.Leader.Should().NotBeNull();
        result.TeamHealth.Leader!.FullName.Should().Be("Beta Leader");
        result.TeamHealth.Leader.TeamName.Should().Be("Squad Beta");
    }

    [Fact]
    public async Task GetMyDashboardAsync_WithTeamId_IndicatorsFromTeamMembers()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var org = new Organization { Id = Guid.NewGuid(), Name = "Org" };
        var member1 = new Employee { Id = Guid.NewGuid(), FullName = "M1 Test", Email = "m1@test.com" };
        var member2 = new Employee { Id = Guid.NewGuid(), FullName = "M2 Test", Email = "m2@test.com" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Squad Gamma", OrganizationId = org.Id };

        context.Organizations.Add(org);
        context.Employees.AddRange(member1, member2);
        context.Teams.Add(team);
        context.OrganizationEmployeeMembers.AddRange(
            new OrganizationEmployeeMember { EmployeeId = member1.Id, OrganizationId = org.Id, Role = EmployeeRole.TeamLeader },
            new OrganizationEmployeeMember { EmployeeId = member2.Id, OrganizationId = org.Id, Role = EmployeeRole.Contributor }
        );
        context.Set<EmployeeTeam>().AddRange(
            new EmployeeTeam { EmployeeId = member1.Id, TeamId = team.Id },
            new EmployeeTeam { EmployeeId = member2.Id, TeamId = team.Id }
        );

        // Only member1 accessed this week
        context.EmployeeAccessLogs.Add(new EmployeeAccessLog
        {
            Id = Guid.NewGuid(),
            EmployeeId = member1.Id,
            OrganizationId = org.Id,
            AccessedAt = DateTime.UtcNow.AddDays(-1)
        });

        await context.SaveChangesAsync();

        var repository = new DashboardReadStore(context);

        // Act
        var result = await repository.GetMyDashboardAsync(member1.Id, teamId: team.Id);

        // Assert
        result.Should().NotBeNull();
        // 1 out of 2 team members accessed = 50%
        result!.TeamHealth.WeeklyAccess.Percentage.Should().Be(50);
    }

    [Fact]
    public async Task GetMyDashboardAsync_WithTeamId_PendingTasksRemainPersonal()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var org = new Organization { Id = Guid.NewGuid(), Name = "Org" };
        var employee = new Employee { Id = Guid.NewGuid(), FullName = "Task Person", Email = "task@test.com" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Squad Delta", OrganizationId = org.Id };

        var mission = new Mission
        {
            Id = Guid.NewGuid(),
            Name = "My Personal Mission",
            OrganizationId = org.Id,
            EmployeeId = employee.Id,
            Status = MissionStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(30)
        };
        var metric = new Indicator
        {
            Id = Guid.NewGuid(),
            Name = "KR",
            MissionId = mission.Id,
            OrganizationId = org.Id,
            Type = IndicatorType.Quantitative
        };
        var checkin = new Checkin
        {
            Id = Guid.NewGuid(),
            IndicatorId = metric.Id,
            EmployeeId = employee.Id,
            OrganizationId = org.Id,
            CheckinDate = DateTime.UtcNow.AddDays(-10),
            ConfidenceLevel = 3,
            Value = 50
        };

        context.Organizations.Add(org);
        context.Employees.Add(employee);
        context.OrganizationEmployeeMembers.Add(new OrganizationEmployeeMember
        {
            EmployeeId = employee.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.Contributor
        });
        context.Teams.Add(team);
        context.Set<EmployeeTeam>().Add(
            new EmployeeTeam { EmployeeId = employee.Id, TeamId = team.Id }
        );
        context.Missions.Add(mission);
        context.Indicators.Add(metric);
        context.Checkins.Add(checkin);
        await context.SaveChangesAsync();

        var repository = new DashboardReadStore(context);

        // Act
        var result = await repository.GetMyDashboardAsync(employee.Id, teamId: team.Id);

        // Assert
        result.Should().NotBeNull();
        // Pending tasks remain personal even when viewing a team
        result!.PendingTasks.Should().HaveCount(1);
        result.PendingTasks[0].Title.Should().Be("My Personal Mission");
    }

    [Fact]
    public async Task GetMyDashboardAsync_WithNonExistentTeamId_ReturnsEmptyTeamHealth()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var org = new Organization { Id = Guid.NewGuid(), Name = "Org" };
        var employee = new Employee { Id = Guid.NewGuid(), FullName = "Alone Person", Email = "alone@test.com" };

        context.Organizations.Add(org);
        context.Employees.Add(employee);
        context.OrganizationEmployeeMembers.Add(new OrganizationEmployeeMember
        {
            EmployeeId = employee.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.Contributor
        });
        await context.SaveChangesAsync();

        var repository = new DashboardReadStore(context);

        // Act
        var result = await repository.GetMyDashboardAsync(employee.Id, teamId: Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.TeamHealth.Leader.Should().BeNull();
        result.TeamHealth.TeamMembers.Should().BeEmpty();
        result.TeamHealth.WeeklyAccess.Percentage.Should().Be(0);
    }

    [Fact]
    public async Task GetMyDashboardAsync_EngagementScore_LowLevel()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var org = new Organization { Id = Guid.NewGuid(), Name = "Org" };
        var employee = new Employee { Id = Guid.NewGuid(), FullName = "Low Engagement", Email = "low@test.com" };

        context.Organizations.Add(org);
        context.Employees.Add(employee);
        context.OrganizationEmployeeMembers.Add(new OrganizationEmployeeMember
        {
            EmployeeId = employee.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.Contributor
        });
        await context.SaveChangesAsync();

        var repository = new DashboardReadStore(context);

        // Act
        var result = await repository.GetMyDashboardAsync(employee.Id, null);

        // Assert
        result.Should().NotBeNull();
        result!.TeamHealth.Engagement.Score.Should().BeLessThan(40);
        result.TeamHealth.Engagement.Level.Should().Be("low");
    }

    [Fact]
    public async Task GetMyDashboardAsync_OrgScopedMission_IncludedInPendingTasks()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var org = new Organization { Id = Guid.NewGuid(), Name = "Org" };
        var employee = new Employee { Id = Guid.NewGuid(), FullName = "Admin User", Email = "admin@test.com" };

        // Org-scoped mission (EmployeeId = null)
        var orgMission = new Mission
        {
            Id = Guid.NewGuid(),
            Name = "Org Mission",
            OrganizationId = org.Id,
            EmployeeId = null,
            Status = MissionStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(30)
        };
        var metric = new Indicator
        {
            Id = Guid.NewGuid(),
            Name = "KR",
            MissionId = orgMission.Id,
            OrganizationId = org.Id,
            Type = IndicatorType.Quantitative
        };
        // Old checkin (> 7 days)
        var checkin = new Checkin
        {
            Id = Guid.NewGuid(),
            IndicatorId = metric.Id,
            EmployeeId = employee.Id,
            OrganizationId = org.Id,
            CheckinDate = DateTime.UtcNow.AddDays(-10),
            ConfidenceLevel = 3,
            Value = 50
        };

        context.Organizations.Add(org);
        context.Employees.Add(employee);
        context.OrganizationEmployeeMembers.Add(new OrganizationEmployeeMember
        {
            EmployeeId = employee.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.TeamLeader
        });
        context.Missions.Add(orgMission);
        context.Indicators.Add(metric);
        context.Checkins.Add(checkin);
        await context.SaveChangesAsync();

        var repository = new DashboardReadStore(context);

        // Act
        var result = await repository.GetMyDashboardAsync(employee.Id, null);

        // Assert
        result.Should().NotBeNull();
        result!.PendingTasks.Should().HaveCount(1);
        result.PendingTasks[0].Title.Should().Be("Org Mission");
    }

    [Fact]
    public async Task GetMyDashboardAsync_MissionsUpdated_IncludesOrgScopedMissions()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var org = new Organization { Id = Guid.NewGuid(), Name = "Org" };
        var employee = new Employee { Id = Guid.NewGuid(), FullName = "Admin User", Email = "admin@test.com" };

        // Personal mission with recent checkin
        var personalMission = new Mission
        {
            Id = Guid.NewGuid(),
            Name = "Personal",
            OrganizationId = org.Id,
            EmployeeId = employee.Id,
            Status = MissionStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(30)
        };
        var personalMetric = new Indicator
        {
            Id = Guid.NewGuid(),
            Name = "KR1",
            MissionId = personalMission.Id,
            OrganizationId = org.Id,
            Type = IndicatorType.Quantitative
        };
        context.Checkins.Add(new Checkin
        {
            Id = Guid.NewGuid(),
            IndicatorId = personalMetric.Id,
            EmployeeId = employee.Id,
            OrganizationId = org.Id,
            CheckinDate = DateTime.UtcNow.AddDays(-1),
            ConfidenceLevel = 4,
            Value = 80
        });

        // Org-scoped mission without checkin
        var orgMission = new Mission
        {
            Id = Guid.NewGuid(),
            Name = "Org Mission",
            OrganizationId = org.Id,
            EmployeeId = null,
            Status = MissionStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(30)
        };
        var orgMetric = new Indicator
        {
            Id = Guid.NewGuid(),
            Name = "KR2",
            MissionId = orgMission.Id,
            OrganizationId = org.Id,
            Type = IndicatorType.Quantitative
        };

        context.Organizations.Add(org);
        context.Employees.Add(employee);
        context.OrganizationEmployeeMembers.Add(new OrganizationEmployeeMember
        {
            EmployeeId = employee.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.TeamLeader
        });
        context.Missions.AddRange(personalMission, orgMission);
        context.Indicators.AddRange(personalMetric, orgMetric);
        await context.SaveChangesAsync();

        var repository = new DashboardReadStore(context);

        // Act
        var result = await repository.GetMyDashboardAsync(employee.Id, null);

        // Assert
        result.Should().NotBeNull();
        // 1 of 2 missions updated = 50%
        result!.TeamHealth.MissionsUpdated.Percentage.Should().Be(50);
    }
}
