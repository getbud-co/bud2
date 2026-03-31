using System.Net;
using System.Net.Http.Json;
using System.Diagnostics;
using Bud.Shared.Contracts;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bud.Api.IntegrationTests.Endpoints;

public class TeamsEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public TeamsEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateGlobalAdminClient();
    }

    private void SetTenantHeader(Guid orgId)
    {
        _client.DefaultRequestHeaders.Remove("X-Tenant-Id");
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", orgId.ToString());
    }

    private async Task<Guid> GetOrCreateAdminLeader()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Bud.Infrastructure.Persistence.ApplicationDbContext>();

        var existingLeader = await dbContext.Employees
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Email == "admin@getbud.co");

        if (existingLeader != null)
        {
            return existingLeader.Id;
        }

        var org = new Organization { Id = Guid.NewGuid(), Name = "getbud.co" };
        dbContext.Organizations.Add(org);

        // Create employee first (without team) so we can reference it as team leader
        var adminLeader = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Administrador",
            Email = "admin@getbud.co",
            Role = EmployeeRole.Leader,
            TeamId = null,
            OrganizationId = org.Id
        };
        dbContext.Employees.Add(adminLeader);

        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "getbud.co",
            OrganizationId = org.Id,
            LeaderId = adminLeader.Id
        };
        dbContext.Teams.Add(team);

        await dbContext.SaveChangesAsync();

        // Now update employee's team
        adminLeader.TeamId = team.Id;
        await dbContext.SaveChangesAsync();

        return adminLeader.Id;
    }

    private async Task<(Organization org, Guid leaderId)> CreateTestHierarchy()
    {
        var organizationDomain = $"test-org-{Guid.NewGuid():N}.com";

        var orgResponse = await _client.PostAsJsonAsync("/api/organizations",
            new CreateOrganizationRequest
            {
                Name = organizationDomain
            });
        var org = await orgResponse.Content.ReadFromJsonAsync<Organization>();

        // Create a Leader employee in the new org (required for team creation)
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Bud.Infrastructure.Persistence.ApplicationDbContext>();
        var leader = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Líder Teste",
            Email = $"leader-{Guid.NewGuid():N}@{organizationDomain}",
            Role = EmployeeRole.Leader,
            OrganizationId = org!.Id,
            TeamId = null
        };
        dbContext.Employees.Add(leader);
        await dbContext.SaveChangesAsync();

        SetTenantHeader(org!.Id);
        return (org!, leader.Id);
    }

    private async Task<Employee> CreateNonLeaderEmployee(Guid organizationId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Bud.Infrastructure.Persistence.ApplicationDbContext>();

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Colaborador Teste",
            Email = $"colaborador-{Guid.NewGuid():N}@example.com",
            Role = EmployeeRole.IndividualContributor,
            OrganizationId = organizationId,
            TeamId = null
        };

        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync();

        return employee;
    }

    #region Create Tests

    [Fact]
    public async Task GetAll_WithInvalidPageSize_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/teams?page=1&pageSize=101");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Detail.Should().Be("O parâmetro 'pageSize' deve estar entre 1 e 100.");
    }

    [Fact]
    public async Task GetAll_ShouldRespondWithinBudget()
    {
        var watch = Stopwatch.StartNew();

        var response = await _client.GetAsync("/api/teams?page=1&pageSize=20");

        watch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        watch.ElapsedMilliseconds.Should().BeLessThan(5000);
    }

    [Fact]
    public async Task Create_WithValidParentTeam_ReturnsCreated()
    {
        // Arrange
        var (org, leaderId) = await CreateTestHierarchy();

        // Create parent team
        SetTenantHeader(org.Id);
        var parentResponse = await _client.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest { Name = "Parent Team", LeaderId = leaderId });
        var parentTeam = await parentResponse.Content.ReadFromJsonAsync<Team>();

        // Create child team
        var request = new CreateTeamRequest
        {
            Name = "Child Team",
            LeaderId = leaderId,
            ParentTeamId = parentTeam!.Id
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/teams", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var team = await response.Content.ReadFromJsonAsync<Team>();
        team.Should().NotBeNull();
        team!.Name.Should().Be("Child Team");
        team.ParentTeamId.Should().Be(parentTeam.Id);
    }

    [Fact]
    public async Task Create_AsNonLeader_ReturnsForbidden()
    {
        // Arrange
        var (org, leaderId) = await CreateTestHierarchy();
        var employee = await CreateNonLeaderEmployee(org.Id);
        var tenantClient = _factory.CreateTenantClient(org.Id, employee.Email, employee.Id);

        var request = new CreateTeamRequest
        {
            Name = "Team NonLeader",
            LeaderId = leaderId
        };

        // Act
        var response = await tenantClient.PostAsJsonAsync("/api/teams", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_WithParentInDifferentOrganization_ReturnsBadRequest()
    {
        // Arrange: create two organizations
        var (orgA, leaderIdA) = await CreateTestHierarchy();
        var (orgB, leaderIdB) = await CreateTestHierarchy();

        // Create parent team in org A
        SetTenantHeader(orgA.Id);
        var parentResponse = await _client.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest { Name = "Parent Team", LeaderId = leaderIdA });
        var parentTeam = await parentResponse.Content.ReadFromJsonAsync<Team>();

        // Try to create child team in org B with parent from org A
        SetTenantHeader(orgB.Id);
        var request = new CreateTeamRequest
        {
            Name = "Child Team",
            LeaderId = leaderIdB,
            ParentTeamId = parentTeam!.Id
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/teams", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Tenant Isolation Tests

    [Fact]
    public async Task GetAll_WithTenantClient_ShouldNotReturnTeamsFromAnotherTenant()
    {
        // Arrange
        var (orgA, leaderIdA) = await CreateTestHierarchy();
        var (orgB, leaderIdB) = await CreateTestHierarchy();

        var teamAName = $"Isolated-A-{Guid.NewGuid():N}";
        var teamBName = $"Isolated-B-{Guid.NewGuid():N}";

        SetTenantHeader(orgA.Id);
        var teamAResponse = await _client.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest { Name = teamAName, LeaderId = leaderIdA });
        teamAResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        SetTenantHeader(orgB.Id);
        var teamBResponse = await _client.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest { Name = teamBName, LeaderId = leaderIdB });
        teamBResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var employeeA = await CreateNonLeaderEmployee(orgA.Id);
        var tenantClientA = _factory.CreateTenantClient(orgA.Id, employeeA.Email, employeeA.Id);

        // Act
        var visibleResponse = await tenantClientA.GetAsync($"/api/teams?search={teamAName}");
        var hiddenResponse = await tenantClientA.GetAsync($"/api/teams?search={teamBName}");

        // Assert
        visibleResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        hiddenResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var visiblePayload = await visibleResponse.Content.ReadFromJsonAsync<PagedResult<Team>>();
        var hiddenPayload = await hiddenResponse.Content.ReadFromJsonAsync<PagedResult<Team>>();

        visiblePayload.Should().NotBeNull();
        hiddenPayload.Should().NotBeNull();
        visiblePayload!.Items.Should().ContainSingle(t => t.Name == teamAName);
        hiddenPayload!.Items.Should().BeEmpty();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Patch_SettingSelfAsParent_ReturnsBadRequest()
    {
        // Arrange
        var (_, leaderId) = await CreateTestHierarchy();

        var createResponse = await _client.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest { Name = "Test Team", LeaderId = leaderId });
        var team = await createResponse.Content.ReadFromJsonAsync<Team>();

        // Try to set itself as parent
        var updateRequest = new PatchTeamRequest
        {
            Name = "Test Team",
            LeaderId = leaderId,
            ParentTeamId = team!.Id
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/teams/{team.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithValidParent_ReturnsOk()
    {
        // Arrange
        var (_, leaderId) = await CreateTestHierarchy();

        // Create two teams
        var team1Response = await _client.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest { Name = "Team 1", LeaderId = leaderId });
        var team1 = await team1Response.Content.ReadFromJsonAsync<Team>();

        var team2Response = await _client.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest { Name = "Team 2", LeaderId = leaderId });
        var team2 = await team2Response.Content.ReadFromJsonAsync<Team>();

        // Update team2 to have team1 as parent
        var updateRequest = new PatchTeamRequest
        {
            Name = "Team 2 Updated",
            LeaderId = leaderId,
            ParentTeamId = team1!.Id
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/teams/{team2!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<Team>();
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Team 2 Updated");
        updated.ParentTeamId.Should().Be(team1.Id);
    }

    [Fact]
    public async Task Update_WhenNotLeader_ReturnsForbidden()
    {
        // Arrange
        var (org, leaderId) = await CreateTestHierarchy();

        var createResponse = await _client.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest { Name = "Team A", LeaderId = leaderId });
        var team = await createResponse.Content.ReadFromJsonAsync<Team>();

        var employee = await CreateNonLeaderEmployee(org.Id);
        var tenantClient = _factory.CreateTenantClient(org.Id, employee.Email, employee.Id);

        var updateRequest = new PatchTeamRequest
        {
            Name = "Team A Updated",
            LeaderId = leaderId
        };

        // Act
        var response = await tenantClient.PatchAsJsonAsync($"/api/teams/{team!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_Team_WithNoMissions_ReturnsNoContent()
    {
        var (_, leaderId) = await CreateTestHierarchy();

        var teamResponse = await _client.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest { Name = "Team deletable", LeaderId = leaderId });
        var team = (await teamResponse.Content.ReadFromJsonAsync<Team>())!;

        // Act
        var response = await _client.DeleteAsync($"/api/teams/{team.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_Team_WithMissionAssignedToMember_ReturnsConflict()
    {
        var (org, leaderId) = await CreateTestHierarchy();

        var teamResponse = await _client.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest { Name = "Team blocked by mission", LeaderId = leaderId });
        var team = (await teamResponse.Content.ReadFromJsonAsync<Team>())!;

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Bud.Infrastructure.Persistence.ApplicationDbContext>();
        dbContext.Missions.Add(new Mission
        {
            Id = Guid.NewGuid(),
            Name = "Mission assigned to team member",
            OrganizationId = org.Id,
            EmployeeId = leaderId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30),
            Status = MissionStatus.Active
        });
        await dbContext.SaveChangesAsync();

        var response = await _client.DeleteAsync($"/api/teams/{team.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Detail.Should().Be("Não é possível excluir o time porque existem metas associadas a ele.");
    }

    [Fact]
    public async Task Delete_WithSubTeams_ReturnsConflict()
    {
        // Arrange
        var (_, leaderId) = await CreateTestHierarchy();

        // Create parent team
        var parentResponse = await _client.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest { Name = "Parent Team", LeaderId = leaderId });
        var parentTeam = await parentResponse.Content.ReadFromJsonAsync<Team>();

        // Create sub-team
        await _client.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest
            {
                Name = "Sub Team",
                LeaderId = leaderId,
                ParentTeamId = parentTeam!.Id
            });

        // Act - Try to delete parent team
        var response = await _client.DeleteAsync($"/api/teams/{parentTeam.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Delete_WithoutSubTeams_ReturnsNoContent()
    {
        // Arrange
        var (_, leaderId) = await CreateTestHierarchy();

        var createResponse = await _client.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest { Name = "Team to Delete", LeaderId = leaderId });
        var team = await createResponse.Content.ReadFromJsonAsync<Team>();

        // Act
        var response = await _client.DeleteAsync($"/api/teams/{team!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's deleted
        var getResponse = await _client.GetAsync($"/api/teams/{team.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WhenNotLeader_ReturnsForbidden()
    {
        // Arrange
        var (org, leaderId) = await CreateTestHierarchy();

        var createResponse = await _client.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest { Name = "Team B", LeaderId = leaderId });
        var team = await createResponse.Content.ReadFromJsonAsync<Team>();

        var employee = await CreateNonLeaderEmployee(org.Id);
        var tenantClient = _factory.CreateTenantClient(org.Id, employee.Email, employee.Id);

        // Act
        var response = await tenantClient.DeleteAsync($"/api/teams/{team!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region GetSubTeams Tests

    [Fact]
    public async Task GetSubTeams_ReturnsSubTeamsOnly()
    {
        // Arrange
        var (_, leaderId) = await CreateTestHierarchy();

        // Create parent team
        var parentResponse = await _client.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest { Name = "Parent Team", LeaderId = leaderId });
        var parentTeam = await parentResponse.Content.ReadFromJsonAsync<Team>();

        // Create sub-teams
        await _client.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest
            {
                Name = "Sub Team 1",
                LeaderId = leaderId,
                ParentTeamId = parentTeam!.Id
            });

        await _client.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest
            {
                Name = "Sub Team 2",
                LeaderId = leaderId,
                ParentTeamId = parentTeam.Id
            });

        // Create unrelated team
        await _client.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest { Name = "Unrelated Team", LeaderId = leaderId });

        // Act
        var response = await _client.GetAsync($"/api/teams/{parentTeam.Id}/subteams");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Team>>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(t => t.ParentTeamId == parentTeam.Id);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var unauthenticatedClient = _factory.CreateClient();

        // Act
        var response = await unauthenticatedClient.GetAsync("/api/teams");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GetEmployees Tests

    [Fact]
    public async Task GetEmployees_ReturnsTeamEmployees()
    {
        // Arrange
        var (org, leaderId) = await CreateTestHierarchy();

        var teamResponse = await _client.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest { Name = "Test Team", LeaderId = leaderId });
        var team = await teamResponse.Content.ReadFromJsonAsync<Team>();

        // Create employees directly in database to keep this setup focused on team listing.
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Bud.Infrastructure.Persistence.ApplicationDbContext>();

        var collab1 = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Employee 1",
            Email = "collab1@example.com",
            Role = EmployeeRole.IndividualContributor,
            OrganizationId = org.Id
        };

        var collab2 = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Employee 2",
            Email = "collab2@example.com",
            Role = EmployeeRole.Leader,
            OrganizationId = org.Id
        };

        dbContext.Employees.AddRange(collab1, collab2);
        dbContext.EmployeeTeams.AddRange(
            new EmployeeTeam { EmployeeId = collab1.Id, TeamId = team!.Id },
            new EmployeeTeam { EmployeeId = collab2.Id, TeamId = team.Id });
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/teams/{team.Id}/employees");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Employee>>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(3);
        result.Items.Should().Contain(c => c.Id == leaderId);
        result.Items.Should().Contain(c => c.Id == collab1.Id);
        result.Items.Should().Contain(c => c.Id == collab2.Id);
    }

    #endregion

    #region Leader EmployeeTeam Sync Tests

    [Fact]
    public async Task Create_ShouldIncludeLeaderInEmployeeSummaries()
    {
        // Arrange
        var (_, leaderId) = await CreateTestHierarchy();

        // Act: create team
        var createResponse = await _client.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest { Name = "Leader Sync Team", LeaderId = leaderId });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var team = await createResponse.Content.ReadFromJsonAsync<Team>();

        // Assert: leader should be in employees summary projection
        var summariesResponse = await _client.GetAsync($"/api/teams/{team!.Id}/employees/lookup");
        summariesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var summaries = await summariesResponse.Content.ReadFromJsonAsync<List<EmployeeLookupResponse>>();
        summaries.Should().NotBeNull();
        summaries.Should().Contain(c => c.Id == leaderId);
    }

    [Fact]
    public async Task PatchEmployees_WithoutLeader_ShouldReturnBadRequest()
    {
        // Arrange
        var (org, leaderId) = await CreateTestHierarchy();

        var createResponse = await _client.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest { Name = "Leader Protected Team", LeaderId = leaderId });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var team = await createResponse.Content.ReadFromJsonAsync<Team>();

        // Create another employee to use in the update
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Bud.Infrastructure.Persistence.ApplicationDbContext>();
        var otherCollab = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Other Employee",
            Email = $"other-{Guid.NewGuid():N}@test-org.com",
            Role = EmployeeRole.IndividualContributor,
            OrganizationId = org.Id
        };
        dbContext.Employees.Add(otherCollab);
        await dbContext.SaveChangesAsync();

        // Act: update employees WITHOUT the leader
        var updateResponse = await _client.PatchAsJsonAsync($"/api/teams/{team!.Id}/employees",
            new PatchTeamEmployeesRequest { EmployeeIds = new List<Guid> { otherCollab.Id } });

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PatchEmployees_WithEmployeeTeamsOnly_KeepsMembershipConsistentAcrossEndpoints()
    {
        var (org, leaderId) = await CreateTestHierarchy();

        var createResponse = await _client.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest { Name = "Consistent Team", LeaderId = leaderId });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var team = await createResponse.Content.ReadFromJsonAsync<Team>();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Bud.Infrastructure.Persistence.ApplicationDbContext>();
        var otherCollab = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Consistent Employee",
            Email = $"consistent-{Guid.NewGuid():N}@test-org.com",
            Role = EmployeeRole.IndividualContributor,
            OrganizationId = org.Id,
            TeamId = null
        };
        dbContext.Employees.Add(otherCollab);
        await dbContext.SaveChangesAsync();

        var updateResponse = await _client.PatchAsJsonAsync($"/api/teams/{team!.Id}/employees",
            new PatchTeamEmployeesRequest { EmployeeIds = new List<Guid> { leaderId, otherCollab.Id } });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var teamEmployeesResponse = await _client.GetAsync($"/api/teams/{team.Id}/employees");
        teamEmployeesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var teamEmployees = await teamEmployeesResponse.Content.ReadFromJsonAsync<PagedResult<Employee>>();
        teamEmployees.Should().NotBeNull();
        teamEmployees!.Items.Should().Contain(c => c.Id == otherCollab.Id);

        var employeesByTeamResponse = await _client.GetAsync($"/api/employees?teamId={team.Id}");
        employeesByTeamResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var employeesByTeam = await employeesByTeamResponse.Content.ReadFromJsonAsync<PagedResult<Employee>>();
        employeesByTeam.Should().NotBeNull();
        employeesByTeam!.Items.Should().Contain(c => c.Id == otherCollab.Id);

        var employeeTeamsResponse = await _client.GetAsync($"/api/employees/{otherCollab.Id}/teams");
        employeeTeamsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var employeeTeams = await employeeTeamsResponse.Content.ReadFromJsonAsync<List<EmployeeTeamResponse>>();
        employeeTeams.Should().NotBeNull();
        employeeTeams.Should().Contain(t => t.Id == team.Id);
    }

    #endregion
}
