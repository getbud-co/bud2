using Bud.Infrastructure.Persistence;
using Bud.Infrastructure.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Bud.Infrastructure.UnitTests.Infrastructure.Repositories;

public sealed class TeamRepositoryTests
{
    private readonly TestTenantProvider _tenantProvider = new() { IsGlobalAdmin = true };

    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options, _tenantProvider);
    }

    private static async Task<Organization> CreateTestOrganization(ApplicationDbContext context, string name = "Test Org")
    {
        var org = new Organization { Id = Guid.NewGuid(), Name = name };
        context.Organizations.Add(org);
        await context.SaveChangesAsync();
        return org;
    }

    private static async Task<Employee> CreateTestEmployee(
        ApplicationDbContext context,
        Guid organizationId,
        string fullName = "Test Employee",
        string email = "test@example.com",
        EmployeeRole role = EmployeeRole.TeamLeader)
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Email = email
        };
        context.Employees.Add(employee);
        context.OrganizationEmployeeMembers.Add(new OrganizationEmployeeMember
        {
            EmployeeId = employee.Id,
            OrganizationId = organizationId,
            Role = role
        });
        await context.SaveChangesAsync();
        return employee;
    }

    private static async Task<Team> CreateTestTeam(
        ApplicationDbContext context,
        Guid organizationId,
        Guid leaderId,
        string name = "Test Team",
        Guid? parentTeamId = null)
    {
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = name,
            OrganizationId = organizationId,
            ParentTeamId = parentTeamId
        };
        context.Teams.Add(team);
        await context.SaveChangesAsync();
        return team;
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenTeamExists_ReturnsTeam()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);
        var org = await CreateTestOrganization(context);
        var leader = await CreateTestEmployee(context, org.Id);
        var team = await CreateTestTeam(context, org.Id, leader.Id);

        // Act
        var result = await repository.GetByIdAsync(team.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(team.Id);
        result.Name.Should().Be("Test Team");
    }

    [Fact]
    public async Task GetByIdAsync_WhenTeamNotFound_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdWithEmployeeTeamsAsync Tests

    [Fact]
    public async Task GetByIdWithEmployeeTeamsAsync_WhenExists_ReturnsTeamWithEmployeeTeams()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);
        var org = await CreateTestOrganization(context);
        var leader = await CreateTestEmployee(context, org.Id);
        var team = await CreateTestTeam(context, org.Id, leader.Id);

        var member = await CreateTestEmployee(context, org.Id, "Member", "member@test.com", EmployeeRole.Contributor);
        context.EmployeeTeams.Add(new EmployeeTeam
        {
            EmployeeId = member.Id,
            TeamId = team.Id,
            AssignedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdWithEmployeeTeamsAsync(team.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(team.Id);
        result.EmployeeTeams.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdWithEmployeeTeamsAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);

        // Act
        var result = await repository.GetByIdWithEmployeeTeamsAsync(Guid.NewGuid());

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
        var repository = new TeamRepository(context);
        var org = await CreateTestOrganization(context);
        var leader = await CreateTestEmployee(context, org.Id);

        for (int i = 0; i < 5; i++)
        {
            await CreateTestTeam(context, org.Id, leader.Id, $"Team {i:D2}");
        }

        // Act
        var result = await repository.GetAllAsync(null, null, 1, 2);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByParentTeamId()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);
        var org = await CreateTestOrganization(context);
        var leader = await CreateTestEmployee(context, org.Id);

        var parentTeam = await CreateTestTeam(context, org.Id, leader.Id, "Parent Team");
        await CreateTestTeam(context, org.Id, leader.Id, "Child Team", parentTeam.Id);
        await CreateTestTeam(context, org.Id, leader.Id, "Root Team");

        // Act
        var result = await repository.GetAllAsync(parentTeam.Id, null, 1, 10);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("Child Team");
    }

    [Fact]
    public async Task GetAllAsync_WithCaseInsensitiveSearch_FiltersCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);
        var org = await CreateTestOrganization(context);
        var leader = await CreateTestEmployee(context, org.Id);

        await CreateTestTeam(context, org.Id, leader.Id, "ALPHA Team");
        await CreateTestTeam(context, org.Id, leader.Id, "Beta Team");

        // Act
        var result = await repository.GetAllAsync(null, "alpha", 1, 10);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("ALPHA Team");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOrderedByName()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);
        var org = await CreateTestOrganization(context);
        var leader = await CreateTestEmployee(context, org.Id);

        await CreateTestTeam(context, org.Id, leader.Id, "Zebra");
        await CreateTestTeam(context, org.Id, leader.Id, "Alpha");
        await CreateTestTeam(context, org.Id, leader.Id, "Mango");

        // Act
        var result = await repository.GetAllAsync(null, null, 1, 10);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items[0].Name.Should().Be("Alpha");
        result.Items[1].Name.Should().Be("Mango");
        result.Items[2].Name.Should().Be("Zebra");
    }

    #endregion

    #region GetSubTeamsAsync Tests

    [Fact]
    public async Task GetSubTeamsAsync_ReturnsSubTeams()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);
        var org = await CreateTestOrganization(context);
        var leader = await CreateTestEmployee(context, org.Id);

        var parentTeam = await CreateTestTeam(context, org.Id, leader.Id, "Parent");
        await CreateTestTeam(context, org.Id, leader.Id, "Sub A", parentTeam.Id);
        await CreateTestTeam(context, org.Id, leader.Id, "Sub B", parentTeam.Id);
        await CreateTestTeam(context, org.Id, leader.Id, "Other Root");

        // Act
        var result = await repository.GetSubTeamsAsync(parentTeam.Id, 1, 10);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(2);
    }

    [Fact]
    public async Task GetSubTeamsAsync_ReturnsPaginatedResults()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);
        var org = await CreateTestOrganization(context);
        var leader = await CreateTestEmployee(context, org.Id);

        var parentTeam = await CreateTestTeam(context, org.Id, leader.Id, "Parent");
        for (int i = 0; i < 5; i++)
        {
            await CreateTestTeam(context, org.Id, leader.Id, $"Sub {i:D2}", parentTeam.Id);
        }

        // Act
        var result = await repository.GetSubTeamsAsync(parentTeam.Id, 1, 2);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(2);
    }

    #endregion

    #region GetEmployeesAsync Tests

    [Fact]
    public async Task GetEmployeesAsync_ReturnsEmployeesViaEmployeeTeams()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);
        var org = await CreateTestOrganization(context);
        var leader = await CreateTestEmployee(context, org.Id);
        var team = await CreateTestTeam(context, org.Id, leader.Id);

        var member1 = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Member A",
            Email = "a@test.com"
        };
        var member2 = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Member B",
            Email = "b@test.com"
        };
        context.Employees.AddRange(member1, member2);
        context.OrganizationEmployeeMembers.AddRange(
            new OrganizationEmployeeMember { EmployeeId = member1.Id, OrganizationId = org.Id },
            new OrganizationEmployeeMember { EmployeeId = member2.Id, OrganizationId = org.Id });
        context.EmployeeTeams.AddRange(
            new EmployeeTeam { EmployeeId = member1.Id, TeamId = team.Id },
            new EmployeeTeam { EmployeeId = member2.Id, TeamId = team.Id });
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetEmployeesAsync(team.Id, 1, 10);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(2);
    }

    [Fact]
    public async Task GetEmployeesAsync_ReturnsPaginatedResults()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);
        var org = await CreateTestOrganization(context);
        var leader = await CreateTestEmployee(context, org.Id);
        var team = await CreateTestTeam(context, org.Id, leader.Id);

        for (int i = 0; i < 5; i++)
        {
            var member = new Employee
            {
                Id = Guid.NewGuid(),
                FullName = $"Member {i:D2}",
                Email = $"member{i}@test.com",
            };

            context.Employees.Add(member);
            context.OrganizationEmployeeMembers.Add(new OrganizationEmployeeMember { EmployeeId = member.Id, OrganizationId = org.Id });
            context.EmployeeTeams.Add(new EmployeeTeam
            {
                EmployeeId = member.Id,
                TeamId = team.Id
            });
        }
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetEmployeesAsync(team.Id, 1, 2);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(2);
    }

    #endregion

    #region GetEmployeeLookupAsync Tests

    [Fact]
    public async Task GetEmployeeLookupAsync_ReturnsEmployeesViaEmployeeTeams()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);
        var org = await CreateTestOrganization(context);
        var leader = await CreateTestEmployee(context, org.Id);
        var team = await CreateTestTeam(context, org.Id, leader.Id);

        var member = await CreateTestEmployee(context, org.Id, "Member", "member@test.com", EmployeeRole.Contributor);
        context.EmployeeTeams.Add(new EmployeeTeam
        {
            EmployeeId = member.Id,
            TeamId = team.Id,
            AssignedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetEmployeeLookupAsync(team.Id);

        // Assert
        result.Should().HaveCount(1);
        result[0].FullName.Should().Be("Member");
    }

    [Fact]
    public async Task GetEmployeeLookupAsync_WhenNoMembers_ReturnsEmpty()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);
        var org = await CreateTestOrganization(context);
        var leader = await CreateTestEmployee(context, org.Id);
        var team = await CreateTestTeam(context, org.Id, leader.Id);

        // Act
        var result = await repository.GetEmployeeLookupAsync(team.Id);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetEligibleEmployeesForAssignmentAsync Tests

    [Fact]
    public async Task GetEligibleEmployeesForAssignmentAsync_ExcludesAlreadyAssigned()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);
        var org = await CreateTestOrganization(context);
        var leader = await CreateTestEmployee(context, org.Id);
        var team = await CreateTestTeam(context, org.Id, leader.Id);

        var assigned = await CreateTestEmployee(context, org.Id, "Assigned", "assigned@test.com", EmployeeRole.Contributor);
        var eligible = await CreateTestEmployee(context, org.Id, "Eligible", "eligible@test.com", EmployeeRole.Contributor);

        context.EmployeeTeams.Add(new EmployeeTeam
        {
            EmployeeId = assigned.Id,
            TeamId = team.Id,
            AssignedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetEligibleEmployeesForAssignmentAsync(team.Id, org.Id, null, 10);

        // Assert
        result.Should().Contain(c => c.EmployeeId == eligible.Id);
        result.Should().NotContain(c => c.EmployeeId == assigned.Id);
    }

    [Fact]
    public async Task GetEligibleEmployeesForAssignmentAsync_RespectsLimit()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);
        var org = await CreateTestOrganization(context);
        var leader = await CreateTestEmployee(context, org.Id);
        var team = await CreateTestTeam(context, org.Id, leader.Id);

        for (int i = 0; i < 5; i++)
        {
            await CreateTestEmployee(context, org.Id, $"Eligible {i}", $"e{i}@test.com", EmployeeRole.Contributor);
        }

        // Act
        var result = await repository.GetEligibleEmployeesForAssignmentAsync(team.Id, org.Id, null, 3);

        // Assert
        result.Should().HaveCountLessOrEqualTo(3);
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_WhenTeamExists_ReturnsTrue()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);
        var org = await CreateTestOrganization(context);
        var leader = await CreateTestEmployee(context, org.Id);
        var team = await CreateTestTeam(context, org.Id, leader.Id);

        // Act
        var result = await repository.ExistsAsync(team.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenTeamNotFound_ReturnsFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);

        // Act
        var result = await repository.ExistsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region HasSubTeamsAsync Tests

    [Fact]
    public async Task HasSubTeamsAsync_WhenHasSubTeams_ReturnsTrue()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);
        var org = await CreateTestOrganization(context);
        var leader = await CreateTestEmployee(context, org.Id);

        var parentTeam = await CreateTestTeam(context, org.Id, leader.Id, "Parent");
        await CreateTestTeam(context, org.Id, leader.Id, "Child", parentTeam.Id);

        // Act
        var result = await repository.HasSubTeamsAsync(parentTeam.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasSubTeamsAsync_WhenNoSubTeams_ReturnsFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);
        var org = await CreateTestOrganization(context);
        var leader = await CreateTestEmployee(context, org.Id);
        var team = await CreateTestTeam(context, org.Id, leader.Id);

        // Act
        var result = await repository.HasSubTeamsAsync(team.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region HasMissionsAsync Tests

    [Fact]
    public async Task HasMissionsAsync_WhenMissionAssignedToTeamMember_ReturnsTrue()
    {
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);
        var org = await CreateTestOrganization(context);
        var leader = await CreateTestEmployee(context, org.Id);
        var team = await CreateTestTeam(context, org.Id, leader.Id);
        context.EmployeeTeams.Add(new EmployeeTeam
        {
            EmployeeId = leader.Id,
            TeamId = team.Id,
            AssignedAt = DateTime.UtcNow
        });

        context.Missions.Add(new Mission
        {
            Id = Guid.NewGuid(),
            Name = "Mission linked by primary team",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30),
            Status = MissionStatus.Active,
            OrganizationId = org.Id,
            EmployeeId = leader.Id
        });
        await context.SaveChangesAsync();

        var result = await repository.HasMissionsAsync(team.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasMissionsAsync_WhenNoMissions_ReturnsFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);
        var org = await CreateTestOrganization(context);
        var leader = await CreateTestEmployee(context, org.Id);
        var team = await CreateTestTeam(context, org.Id, leader.Id);

        // Act
        var result = await repository.HasMissionsAsync(team.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasMissionsAsync_WhenMissionAssignedToAdditionalTeamMember_ReturnsTrue()
    {
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);
        var org = await CreateTestOrganization(context);
        var leader = await CreateTestEmployee(context, org.Id);
        var team = await CreateTestTeam(context, org.Id, leader.Id);
        var member = await CreateTestEmployee(context, org.Id, "Member", "member@test.com", EmployeeRole.Contributor);

        context.EmployeeTeams.Add(new EmployeeTeam
        {
            EmployeeId = member.Id,
            TeamId = team.Id,
            AssignedAt = DateTime.UtcNow
        });
        context.Missions.Add(new Mission
        {
            Id = Guid.NewGuid(),
            Name = "Mission linked by additional team",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30),
            Status = MissionStatus.Active,
            OrganizationId = org.Id,
            EmployeeId = member.Id
        });
        await context.SaveChangesAsync();

        var result = await repository.HasMissionsAsync(team.Id);

        result.Should().BeTrue();
    }

    #endregion

    #region AddAsync / RemoveAsync / SaveChangesAsync Tests

    [Fact]
    public async Task AddAsync_PersistsTeam()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);
        var org = await CreateTestOrganization(context);
        var leader = await CreateTestEmployee(context, org.Id);

        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "New Team",
            OrganizationId = org.Id
        };

        // Act
        await repository.AddAsync(team);
        await repository.SaveChangesAsync();

        // Assert
        var persisted = await context.Teams.FindAsync(team.Id);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("New Team");
    }

    [Fact]
    public async Task RemoveAsync_DeletesTeam()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TeamRepository(context);
        var org = await CreateTestOrganization(context);
        var leader = await CreateTestEmployee(context, org.Id);
        var team = await CreateTestTeam(context, org.Id, leader.Id, "To Delete");

        // Re-fetch tracked entity
        var tracked = await context.Teams.FirstAsync(t => t.Id == team.Id);

        // Act
        await repository.RemoveAsync(tracked);
        await repository.SaveChangesAsync();

        // Assert
        var persisted = await context.Teams.FindAsync(team.Id);
        persisted.Should().BeNull();
    }

    #endregion
}
