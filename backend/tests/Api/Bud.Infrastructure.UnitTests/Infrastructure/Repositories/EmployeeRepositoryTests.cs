using Bud.Infrastructure.Persistence;
using Bud.Infrastructure.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Bud.Infrastructure.UnitTests.Infrastructure.Repositories;

public sealed class EmployeeRepositoryTests
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

    private static async Task<Team> CreateTestTeam(
        ApplicationDbContext context, Guid organizationId, string name = "Test Team")
    {
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = name,
            OrganizationId = organizationId,
        };
        context.Teams.Add(team);
        await context.SaveChangesAsync();
        return team;
    }

    private static Employee CreateTestEmployee(
        string fullName = "Test Employee",
        string email = "test@example.com")
    {
        return new Employee
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Email = email,
        };
    }

    private static OrganizationEmployeeMember CreateTestMember(
        Guid employeeId,
        Guid organizationId,
        EmployeeRole role = EmployeeRole.Contributor,
        Guid? leaderId = null)
    {
        return new OrganizationEmployeeMember
        {
            EmployeeId = employeeId,
            OrganizationId = organizationId,
            Role = role,
            LeaderId = leaderId,
        };
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenEmployeeExists_ReturnsEmployee()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var employee = CreateTestEmployee();
        var member = CreateTestMember(employee.Id, org.Id);
        context.Employees.Add(employee);
        context.OrganizationEmployeeMembers.Add(member);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(employee.Id);

        // Assert
        result.Should().NotBeNull();
        result!.EmployeeId.Should().Be(employee.Id);
        result.Employee.FullName.Should().Be("Test Employee");
    }

    [Fact]
    public async Task GetByIdAsync_WhenEmployeeNotFound_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdWithEmployeeTeamsAsync Tests

    [Fact]
    public async Task GetByIdWithEmployeeTeamsAsync_WhenExists_ReturnsEmployeeWithTeams()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var leader = CreateTestEmployee("Leader", "leader@test.com");
        var leaderMember = CreateTestMember(leader.Id, org.Id, EmployeeRole.TeamLeader);
        context.Employees.Add(leader);
        context.OrganizationEmployeeMembers.Add(leaderMember);
        await context.SaveChangesAsync();

        var team = await CreateTestTeam(context, org.Id);

        var employee = CreateTestEmployee("Member", "member@test.com");
        var employeeMember = CreateTestMember(employee.Id, org.Id);
        context.Employees.Add(employee);
        context.OrganizationEmployeeMembers.Add(employeeMember);
        await context.SaveChangesAsync();

        context.EmployeeTeams.Add(new EmployeeTeam
        {
            EmployeeId = employee.Id,
            TeamId = team.Id,
            AssignedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdWithEmployeeTeamsAsync(employee.Id);

        // Assert
        result.Should().NotBeNull();
        result!.EmployeeId.Should().Be(employee.Id);
        result.Employee.EmployeeTeams.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdWithEmployeeTeamsAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);

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
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        for (int i = 0; i < 5; i++)
        {
            var emp = CreateTestEmployee($"Employee {i:D2}", $"collab{i}@test.com");
            context.Employees.Add(emp);
            context.OrganizationEmployeeMembers.Add(CreateTestMember(emp.Id, org.Id));
        }
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync(null, null, 1, 2);

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
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var alice = CreateTestEmployee("ALICE Smith", "alice@test.com");
        var bob = CreateTestEmployee("Bob Jones", "bob@test.com");
        context.Employees.AddRange(alice, bob);
        context.OrganizationEmployeeMembers.AddRange(
            CreateTestMember(alice.Id, org.Id),
            CreateTestMember(bob.Id, org.Id));
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync(null, "alice", 1, 10);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Employee.FullName.Should().Be("ALICE Smith");
    }

    [Fact]
    public async Task GetAllAsync_FiltersByTeamId()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var leader = CreateTestEmployee("Leader", "leader@test.com");
        var leaderMember = CreateTestMember(leader.Id, org.Id, EmployeeRole.TeamLeader);
        context.Employees.Add(leader);
        context.OrganizationEmployeeMembers.Add(leaderMember);
        await context.SaveChangesAsync();

        var team = await CreateTestTeam(context, org.Id);

        var memberA = CreateTestEmployee("Member A", "a@test.com");
        var memberB = CreateTestEmployee("Member B", "b@test.com");
        context.Employees.AddRange(memberA, memberB);
        context.OrganizationEmployeeMembers.AddRange(
            CreateTestMember(memberA.Id, org.Id),
            CreateTestMember(memberB.Id, org.Id));
        await context.SaveChangesAsync();

        context.EmployeeTeams.Add(new EmployeeTeam
        {
            EmployeeId = memberA.Id,
            TeamId = team.Id,
            AssignedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync(team.Id, null, 1, 10);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Employee.FullName.Should().Be("Member A");
    }

    #endregion

    #region GetLeadersAsync Tests

    [Fact]
    public async Task GetLeadersAsync_ReturnsOnlyLeaders()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var leaderEmp = CreateTestEmployee("Leader One", "leader1@test.com");
        var icEmp = CreateTestEmployee("IC One", "ic1@test.com");
        context.Employees.AddRange(leaderEmp, icEmp);
        context.OrganizationEmployeeMembers.AddRange(
            CreateTestMember(leaderEmp.Id, org.Id, EmployeeRole.TeamLeader),
            CreateTestMember(icEmp.Id, org.Id, EmployeeRole.Contributor));
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetLeadersAsync(null);

        // Assert
        result.Should().HaveCount(1);
        result[0].Employee.FullName.Should().Be("Leader One");
    }

    [Fact]
    public async Task GetLeadersAsync_FiltersByOrganizationId()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org1 = await CreateTestOrganization(context, "Org 1");
        var org2 = await CreateTestOrganization(context, "Org 2");

        var leaderOrg1 = CreateTestEmployee("Leader Org1", "leader1@test.com");
        var leaderOrg2 = CreateTestEmployee("Leader Org2", "leader2@test.com");
        context.Employees.AddRange(leaderOrg1, leaderOrg2);
        context.OrganizationEmployeeMembers.AddRange(
            CreateTestMember(leaderOrg1.Id, org1.Id, EmployeeRole.TeamLeader),
            CreateTestMember(leaderOrg2.Id, org2.Id, EmployeeRole.TeamLeader));
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetLeadersAsync(org1.Id);

        // Assert
        result.Should().HaveCount(1);
        result[0].Employee.FullName.Should().Be("Leader Org1");
    }

    #endregion

    #region GetSubordinatesAsync Tests

    [Fact]
    public async Task GetSubordinatesAsync_ReturnsDirectSubordinates()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var leader = CreateTestEmployee("Leader", "leader@test.com");
        context.Employees.Add(leader);
        context.OrganizationEmployeeMembers.Add(CreateTestMember(leader.Id, org.Id, EmployeeRole.TeamLeader));
        await context.SaveChangesAsync();

        var sub1 = CreateTestEmployee("Sub 1", "sub1@test.com");
        var sub2 = CreateTestEmployee("Sub 2", "sub2@test.com");
        context.Employees.AddRange(sub1, sub2);
        context.OrganizationEmployeeMembers.AddRange(
            CreateTestMember(sub1.Id, org.Id, leaderId: leader.Id),
            CreateTestMember(sub2.Id, org.Id, leaderId: leader.Id));
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetSubordinatesAsync(leader.Id, 1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSubordinatesAsync_RespectsMaxDepth()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var leader = CreateTestEmployee("Leader", "leader@test.com");
        context.Employees.Add(leader);
        context.OrganizationEmployeeMembers.Add(CreateTestMember(leader.Id, org.Id, EmployeeRole.TeamLeader));
        await context.SaveChangesAsync();

        var mid = CreateTestEmployee("Mid Manager", "mid@test.com");
        context.Employees.Add(mid);
        context.OrganizationEmployeeMembers.Add(CreateTestMember(mid.Id, org.Id, EmployeeRole.TeamLeader, leader.Id));
        await context.SaveChangesAsync();

        var deep = CreateTestEmployee("Deep IC", "deep@test.com");
        context.Employees.Add(deep);
        context.OrganizationEmployeeMembers.Add(CreateTestMember(deep.Id, org.Id, leaderId: mid.Id));
        await context.SaveChangesAsync();

        // Act - depth 1 should only return direct subordinates
        var result = await repository.GetSubordinatesAsync(leader.Id, 1);

        // Assert
        result.Should().HaveCount(1);
        result[0].Employee.FullName.Should().Be("Mid Manager");
    }

    [Fact]
    public async Task GetSubordinatesAsync_WhenNoSubordinates_ReturnsEmpty()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var employee = CreateTestEmployee();
        context.Employees.Add(employee);
        context.OrganizationEmployeeMembers.Add(CreateTestMember(employee.Id, org.Id));
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetSubordinatesAsync(employee.Id, 5);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetTeamsAsync Tests

    [Fact]
    public async Task GetTeamsAsync_ReturnsTeamsForEmployee()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var leader = CreateTestEmployee("Leader", "leader@test.com");
        context.Employees.Add(leader);
        context.OrganizationEmployeeMembers.Add(CreateTestMember(leader.Id, org.Id, EmployeeRole.TeamLeader));
        await context.SaveChangesAsync();

        var team1 = await CreateTestTeam(context, org.Id, "Team Alpha");
        var team2 = await CreateTestTeam(context, org.Id, "Team Beta");

        var employee = CreateTestEmployee("Member", "member@test.com");
        context.Employees.Add(employee);
        context.OrganizationEmployeeMembers.Add(CreateTestMember(employee.Id, org.Id));
        await context.SaveChangesAsync();

        context.EmployeeTeams.AddRange(
            new EmployeeTeam { EmployeeId = employee.Id, TeamId = team1.Id, AssignedAt = DateTime.UtcNow },
            new EmployeeTeam { EmployeeId = employee.Id, TeamId = team2.Id, AssignedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetTeamsAsync(employee.Id);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTeamsAsync_WhenNoTeams_ReturnsEmpty()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var employee = CreateTestEmployee();
        context.Employees.Add(employee);
        context.OrganizationEmployeeMembers.Add(CreateTestMember(employee.Id, org.Id));
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetTeamsAsync(employee.Id);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetEligibleTeamsForAssignmentAsync Tests

    [Fact]
    public async Task GetEligibleTeamsForAssignmentAsync_ExcludesAlreadyAssignedTeams()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var leader = CreateTestEmployee("Leader", "leader@test.com");
        context.Employees.Add(leader);
        context.OrganizationEmployeeMembers.Add(CreateTestMember(leader.Id, org.Id, EmployeeRole.TeamLeader));
        await context.SaveChangesAsync();

        var teamAssigned = await CreateTestTeam(context, org.Id, "Assigned Team");
        var teamEligible = await CreateTestTeam(context, org.Id, "Eligible Team");

        var employee = CreateTestEmployee("Member", "member@test.com");
        context.Employees.Add(employee);
        context.OrganizationEmployeeMembers.Add(CreateTestMember(employee.Id, org.Id));
        await context.SaveChangesAsync();

        context.EmployeeTeams.Add(new EmployeeTeam
        {
            EmployeeId = employee.Id,
            TeamId = teamAssigned.Id,
            AssignedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetEligibleTeamsForAssignmentAsync(employee.Id, org.Id, null, 10);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Eligible Team");
    }

    #endregion

    #region GetLookupAsync Tests

    [Fact]
    public async Task GetLookupAsync_ReturnsEmployeesUpToLimit()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        for (int i = 0; i < 5; i++)
        {
            var emp = CreateTestEmployee($"Collab {i:D2}", $"c{i}@test.com");
            context.Employees.Add(emp);
            context.OrganizationEmployeeMembers.Add(CreateTestMember(emp.Id, org.Id));
        }
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetLookupAsync(null, 3);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetLookupAsync_WithSearch_FiltersCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var alice = CreateTestEmployee("Alice Wonder", "alice@test.com");
        var bob = CreateTestEmployee("Bob Builder", "bob@test.com");
        context.Employees.AddRange(alice, bob);
        context.OrganizationEmployeeMembers.AddRange(
            CreateTestMember(alice.Id, org.Id),
            CreateTestMember(bob.Id, org.Id));
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetLookupAsync("alice", 10);

        // Assert
        result.Should().HaveCount(1);
        result[0].Employee.FullName.Should().Be("Alice Wonder");
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_WhenExists_ReturnsTrue()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var employee = CreateTestEmployee();
        context.Employees.Add(employee);
        context.OrganizationEmployeeMembers.Add(CreateTestMember(employee.Id, org.Id));
        await context.SaveChangesAsync();

        // Act
        var result = await repository.ExistsAsync(employee.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenNotFound_ReturnsFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);

        // Act
        var result = await repository.ExistsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsEmailUniqueAsync Tests

    [Fact]
    public async Task IsEmailUniqueAsync_WhenEmailNotUsed_ReturnsTrue()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);

        // Act
        var result = await repository.IsEmailUniqueAsync("unique@test.com", null);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsEmailUniqueAsync_WhenEmailUsed_ReturnsFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var emp = CreateTestEmployee("Existing", "taken@test.com");
        context.Employees.Add(emp);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.IsEmailUniqueAsync("taken@test.com", null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsEmailUniqueAsync_WhenEmailUsedBySameId_ReturnsTrue()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var employee = CreateTestEmployee("Self", "self@test.com");
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.IsEmailUniqueAsync("self@test.com", employee.Id);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region HasSubordinatesAsync Tests

    [Fact]
    public async Task HasSubordinatesAsync_WhenHasSubordinates_ReturnsTrue()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var leader = CreateTestEmployee("Leader", "leader@test.com");
        context.Employees.Add(leader);
        context.OrganizationEmployeeMembers.Add(CreateTestMember(leader.Id, org.Id, EmployeeRole.TeamLeader));
        await context.SaveChangesAsync();

        var sub = CreateTestEmployee("Sub", "sub@test.com");
        context.Employees.Add(sub);
        context.OrganizationEmployeeMembers.Add(CreateTestMember(sub.Id, org.Id, leaderId: leader.Id));
        await context.SaveChangesAsync();

        // Act
        var result = await repository.HasSubordinatesAsync(leader.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasSubordinatesAsync_WhenNoSubordinates_ReturnsFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var employee = CreateTestEmployee();
        context.Employees.Add(employee);
        context.OrganizationEmployeeMembers.Add(CreateTestMember(employee.Id, org.Id));
        await context.SaveChangesAsync();

        // Act
        var result = await repository.HasSubordinatesAsync(employee.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region HasMissionsAsync Tests

    [Fact]
    public async Task HasMissionsAsync_WhenHasMissions_ReturnsTrue()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var employee = CreateTestEmployee();
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        context.Missions.Add(new Mission
        {
            Id = Guid.NewGuid(),
            Name = "Test Mission",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            Status = MissionStatus.Planned,
            OrganizationId = org.Id,
            EmployeeId = employee.Id
        });
        await context.SaveChangesAsync();

        // Act
        var result = await repository.HasMissionsAsync(employee.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasMissionsAsync_WhenNoMissions_ReturnsFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var employee = CreateTestEmployee();
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.HasMissionsAsync(employee.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CountTeamsByIdsAndOrganizationAsync Tests

    [Fact]
    public async Task CountTeamsByIdsAndOrganizationAsync_ReturnsCorrectCount()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var leader = CreateTestEmployee("Leader", "leader@test.com");
        context.Employees.Add(leader);
        context.OrganizationEmployeeMembers.Add(CreateTestMember(leader.Id, org.Id, EmployeeRole.TeamLeader));
        await context.SaveChangesAsync();

        var team1 = await CreateTestTeam(context, org.Id, "Team 1");
        var team2 = await CreateTestTeam(context, org.Id, "Team 2");

        // Act
        var result = await repository.CountTeamsByIdsAndOrganizationAsync(
            new List<Guid> { team1.Id, team2.Id, Guid.NewGuid() }, org.Id);

        // Assert
        result.Should().Be(2);
    }

    #endregion

    #region CountByIdsAndOrganizationAsync Tests

    [Fact]
    public async Task CountByIdsAndOrganizationAsync_ReturnsCorrectCount()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var c1 = CreateTestEmployee("C1", "c1@test.com");
        var c2 = CreateTestEmployee("C2", "c2@test.com");
        context.Employees.AddRange(c1, c2);
        context.OrganizationEmployeeMembers.AddRange(
            CreateTestMember(c1.Id, org.Id),
            CreateTestMember(c2.Id, org.Id));
        await context.SaveChangesAsync();

        // Act
        var result = await repository.CountByIdsAndOrganizationAsync(
            new List<Guid> { c1.Id, c2.Id, Guid.NewGuid() }, org.Id);

        // Assert
        result.Should().Be(2);
    }

    #endregion

    #region IsValidLeaderAsync Tests

    [Fact]
    public async Task IsValidLeaderAsync_WhenIsLeaderInOrganization_ReturnsTrue()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var leader = CreateTestEmployee("Leader", "leader@test.com");
        context.Employees.Add(leader);
        context.OrganizationEmployeeMembers.Add(CreateTestMember(leader.Id, org.Id, EmployeeRole.TeamLeader));
        await context.SaveChangesAsync();

        // Act
        var result = await repository.IsValidLeaderAsync(leader.Id, org.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsValidLeaderAsync_WhenNotLeaderRole_ReturnsFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var ic = CreateTestEmployee("IC", "ic@test.com");
        context.Employees.Add(ic);
        context.OrganizationEmployeeMembers.Add(CreateTestMember(ic.Id, org.Id, EmployeeRole.Contributor));
        await context.SaveChangesAsync();

        // Act
        var result = await repository.IsValidLeaderAsync(ic.Id, org.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsValidLeaderAsync_WhenNotFound_ReturnsFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);

        // Act
        var result = await repository.IsValidLeaderAsync(Guid.NewGuid(), null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsValidLeaderAsync_WhenLeaderInDifferentOrganization_ReturnsFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org1 = await CreateTestOrganization(context, "Org 1");
        var org2 = await CreateTestOrganization(context, "Org 2");

        var leader = CreateTestEmployee("Leader", "leader@test.com");
        context.Employees.Add(leader);
        context.OrganizationEmployeeMembers.Add(CreateTestMember(leader.Id, org1.Id, EmployeeRole.TeamLeader));
        await context.SaveChangesAsync();

        // Act
        var result = await repository.IsValidLeaderAsync(leader.Id, org2.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region AddAsync / RemoveAsync / SaveChangesAsync Tests

    [Fact]
    public async Task AddAsync_PersistsEmployee()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var employee = CreateTestEmployee("New Collab", "new@test.com");
        var member = CreateTestMember(employee.Id, org.Id);

        // Act
        await repository.AddAsync(employee, member);
        await repository.SaveChangesAsync();

        // Assert
        var persisted = await context.Employees.FindAsync(employee.Id);
        persisted.Should().NotBeNull();
        persisted!.FullName.Should().Be("New Collab");

        var persistedMember = await context.OrganizationEmployeeMembers
            .FirstOrDefaultAsync(m => m.EmployeeId == employee.Id);
        persistedMember.Should().NotBeNull();
    }

    [Fact]
    public async Task RemoveAsync_DeletesEmployee()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EmployeeRepository(context);
        var org = await CreateTestOrganization(context);

        var employee = CreateTestEmployee("To Delete", "delete@test.com");
        var member = CreateTestMember(employee.Id, org.Id);
        context.Employees.Add(employee);
        context.OrganizationEmployeeMembers.Add(member);
        await context.SaveChangesAsync();

        // Re-fetch tracked OEM entity
        var tracked = await context.OrganizationEmployeeMembers.FirstAsync(m => m.EmployeeId == employee.Id);

        // Act
        await repository.RemoveAsync(tracked);
        await repository.SaveChangesAsync();

        // Assert
        var persisted = await context.OrganizationEmployeeMembers
            .FirstOrDefaultAsync(m => m.EmployeeId == employee.Id);
        persisted.Should().BeNull();
    }

    #endregion
}
