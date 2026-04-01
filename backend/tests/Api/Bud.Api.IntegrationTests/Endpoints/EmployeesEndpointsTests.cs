using System.Net;
using System.Net.Http.Json;
using Bud.Shared.Contracts;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bud.Api.IntegrationTests.Endpoints;

public class EmployeesEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _adminClient;

    public EmployeesEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _adminClient = factory.CreateGlobalAdminClient();
    }

    [Fact]
    public async Task GetAll_WithInvalidPageSize_ReturnsBadRequest()
    {
        var response = await _adminClient.GetAsync("/api/employees?page=1&pageSize=101");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Detail.Should().Be("O parâmetro 'pageSize' deve estar entre 1 e 100.");
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        var unauthenticatedClient = _factory.CreateClient();

        var response = await unauthenticatedClient.GetAsync("/api/employees");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_AsNonLeader_ReturnsForbidden()
    {
        await GetOrCreateAdminLeader();
        var orgResponse = await _adminClient.PostAsJsonAsync("/api/organizations",
            new CreateOrganizationRequest
            {
                Name = $"test-{Guid.NewGuid():N}.com"
            });
        var org = (await orgResponse.Content.ReadFromJsonAsync<Organization>())!;

        var employee = await CreateNonLeaderEmployee(org.Id);
        var tenantClient = _factory.CreateTenantClient(org.Id, employee.Email, employee.Id);

        var request = new CreateEmployeeRequest
        {
            FullName = "Novo Colaborador",
            Email = $"novo-{Guid.NewGuid():N}@test.com",
            Role = Bud.Shared.Kernel.Enums.EmployeeRole.IndividualContributor
        };

        var response = await tenantClient.PostAsJsonAsync("/api/employees", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_WithTeamId_AssignsMembershipAndAppearsInTeamEmployees()
    {
        await GetOrCreateAdminLeader();
        var orgResponse = await _adminClient.PostAsJsonAsync("/api/organizations",
            new CreateOrganizationRequest
            {
                Name = $"employee-team-{Guid.NewGuid():N}.com"
            });
        var org = (await orgResponse.Content.ReadFromJsonAsync<Organization>())!;

        var leader = await CreateLeaderEmployee(org.Id);
        var leaderClient = _factory.CreateTenantClient(org.Id, leader.Email, leader.Id);

        var teamResponse = await leaderClient.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest
            {
                Name = $"Time {Guid.NewGuid():N}",
                LeaderId = leader.Id
            });
        teamResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var team = (await teamResponse.Content.ReadFromJsonAsync<Team>())!;

        var createRequest = new CreateEmployeeRequest
        {
            FullName = "Colaborador com Time",
            Email = $"employee-team-{Guid.NewGuid():N}@test.com",
            Role = Bud.Shared.Kernel.Enums.EmployeeRole.IndividualContributor,
            TeamId = team.Id
        };

        var createResponse = await leaderClient.PostAsJsonAsync("/api/employees", createRequest);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var employee = await createResponse.Content.ReadFromJsonAsync<EmployeeResponse>();
        employee.Should().NotBeNull();
        var employeeId = employee!.Id;

        var employeeTeamsResponse = await leaderClient.GetAsync($"/api/employees/{employeeId}/teams");
        employeeTeamsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var employeeTeams = await employeeTeamsResponse.Content.ReadFromJsonAsync<List<EmployeeTeamResponse>>();
        employeeTeams.Should().NotBeNull();
        employeeTeams!.Should().Contain(t => t.Id == team.Id);

        var teamEmployeesResponse = await leaderClient.GetAsync($"/api/teams/{team.Id}/employees");

        teamEmployeesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedEmployees = await teamEmployeesResponse.Content.ReadFromJsonAsync<PagedResult<Employee>>();
        pagedEmployees.Should().NotBeNull();
        pagedEmployees!.Items.Should().Contain(e => e.Id == employee.Id);
    }

    [Fact]
    public async Task Update_AsNonLeader_ReturnsForbidden()
    {
        await GetOrCreateAdminLeader();
        var orgResponse = await _adminClient.PostAsJsonAsync("/api/organizations",
            new CreateOrganizationRequest
            {
                Name = $"test-{Guid.NewGuid():N}.com"
            });
        var org = (await orgResponse.Content.ReadFromJsonAsync<Organization>())!;

        var nonLeader = await CreateNonLeaderEmployee(org.Id);
        var target = await CreateEmployee(org.Id);
        var tenantClient = _factory.CreateTenantClient(org.Id, nonLeader.Email, nonLeader.Id);

        var request = new PatchEmployeeRequest
        {
            FullName = "Colaborador Atualizado",
            Email = $"atualizado-{Guid.NewGuid():N}@test.com",
            Role = Bud.Shared.Kernel.Enums.EmployeeRole.IndividualContributor
        };

        var response = await tenantClient.PatchAsJsonAsync($"/api/employees/{target.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_WithLeaderFromAnotherOrganization_ReturnsBadRequest()
    {
        await GetOrCreateAdminLeader();

        var org1Response = await _adminClient.PostAsJsonAsync("/api/organizations",
            new CreateOrganizationRequest
            {
                Name = $"employee-org1-{Guid.NewGuid():N}.com"
            });
        var org1 = (await org1Response.Content.ReadFromJsonAsync<Organization>())!;

        var org2Response = await _adminClient.PostAsJsonAsync("/api/organizations",
            new CreateOrganizationRequest
            {
                Name = $"employee-org2-{Guid.NewGuid():N}.com"
            });
        var org2 = (await org2Response.Content.ReadFromJsonAsync<Organization>())!;

        var leaderInOrg1 = await CreateLeaderEmployee(org1.Id);
        var target = await CreateEmployee(org1.Id);
        var foreignLeader = await CreateLeaderEmployee(org2.Id);
        var leaderClient = _factory.CreateTenantClient(org1.Id, leaderInOrg1.Email, leaderInOrg1.Id);

        var request = new PatchEmployeeRequest
        {
            LeaderId = foreignLeader.Id
        };

        var response = await leaderClient.PatchAsJsonAsync($"/api/employees/{target.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("LeaderId.Value");
    }

    [Fact]
    public async Task Delete_WithAssociatedMissions_ReturnsConflict()
    {
        // Arrange
        await GetOrCreateAdminLeader();
        var orgResponse = await _adminClient.PostAsJsonAsync("/api/organizations",
            new CreateOrganizationRequest
            {
                Name = $"test-{Guid.NewGuid():N}.com"
            });
        var org = (await orgResponse.Content.ReadFromJsonAsync<Organization>())!;

        var employee = await CreateEmployee(org.Id);

        // Create mission scoped to employee via DbContext
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Bud.Infrastructure.Persistence.ApplicationDbContext>();
        var mission = new Mission
        {
            Id = Guid.NewGuid(),
            Name = "Missão Colaborador",
            OrganizationId = org.Id,
            EmployeeId = employee.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30),
            Status = MissionStatus.Active
        };
        dbContext.Missions.Add(mission);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _adminClient.DeleteAsync($"/api/employees/{employee.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Delete_AsNonLeader_ReturnsForbidden()
    {
        await GetOrCreateAdminLeader();
        var orgResponse = await _adminClient.PostAsJsonAsync("/api/organizations",
            new CreateOrganizationRequest
            {
                Name = $"test-{Guid.NewGuid():N}.com"
            });
        var org = (await orgResponse.Content.ReadFromJsonAsync<Organization>())!;

        var nonLeader = await CreateNonLeaderEmployee(org.Id);
        var target = await CreateEmployee(org.Id);
        var tenantClient = _factory.CreateTenantClient(org.Id, nonLeader.Email, nonLeader.Id);

        var response = await tenantClient.DeleteAsync($"/api/employees/{target.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetSubordinates_ValidLeader_ReturnsOk()
    {
        await GetOrCreateAdminLeader();
        var orgResponse = await _adminClient.PostAsJsonAsync("/api/organizations",
            new CreateOrganizationRequest
            {
                Name = $"test-{Guid.NewGuid():N}.com"
            });
        var org = (await orgResponse.Content.ReadFromJsonAsync<Organization>())!;

        var leader = await CreateLeaderEmployee(org.Id);
        var sub = await CreateSubordinate(org.Id, leader.Id);

        var response = await _adminClient.GetAsync($"/api/employees/{leader.Id}/subordinates");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var nodes = await response.Content.ReadFromJsonAsync<List<EmployeeSubordinateResponse>>();
        nodes.Should().NotBeNull();
        nodes.Should().HaveCount(1);
        nodes![0].FullName.Should().Be(sub.FullName);
    }

    [Fact]
    public async Task GetSubordinates_NonExistent_ReturnsNotFound()
    {
        var response = await _adminClient.GetAsync($"/api/employees/{Guid.NewGuid()}/subordinates");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSubordinates_Recursive_ReturnsNestedTree()
    {
        await GetOrCreateAdminLeader();
        var orgResponse = await _adminClient.PostAsJsonAsync("/api/organizations",
            new CreateOrganizationRequest
            {
                Name = $"test-{Guid.NewGuid():N}.com"
            });
        var org = (await orgResponse.Content.ReadFromJsonAsync<Organization>())!;

        var topLeader = await CreateLeaderEmployee(org.Id);
        var midLeader = await CreateLeaderSubordinate(org.Id, topLeader.Id);
        var contributor = await CreateSubordinate(org.Id, midLeader.Id);

        var response = await _adminClient.GetAsync($"/api/employees/{topLeader.Id}/subordinates");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var nodes = await response.Content.ReadFromJsonAsync<List<EmployeeSubordinateResponse>>();
        nodes.Should().HaveCount(1);
        nodes![0].FullName.Should().Be(midLeader.FullName);
        nodes[0].Children.Should().HaveCount(1);
        nodes[0].Children[0].FullName.Should().Be(contributor.FullName);
    }

    private async Task<Employee> CreateLeaderEmployee(Guid organizationId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Bud.Infrastructure.Persistence.ApplicationDbContext>();

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = $"Líder {Guid.NewGuid():N}",
            Email = $"leader-{Guid.NewGuid():N}@test.com",
            Role = EmployeeRole.Leader,
            OrganizationId = organizationId
        };

        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync();

        return employee;
    }

    private async Task<Employee> CreateLeaderSubordinate(Guid organizationId, Guid leaderId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Bud.Infrastructure.Persistence.ApplicationDbContext>();

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = $"Sub-líder {Guid.NewGuid():N}",
            Email = $"subleader-{Guid.NewGuid():N}@test.com",
            Role = EmployeeRole.Leader,
            OrganizationId = organizationId,
            LeaderId = leaderId
        };

        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync();

        return employee;
    }

    private async Task<Employee> CreateSubordinate(Guid organizationId, Guid leaderId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Bud.Infrastructure.Persistence.ApplicationDbContext>();

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = $"Liderado {Guid.NewGuid():N}",
            Email = $"sub-{Guid.NewGuid():N}@test.com",
            Role = EmployeeRole.IndividualContributor,
            OrganizationId = organizationId,
            LeaderId = leaderId
        };

        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync();

        return employee;
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

        var adminLeader = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Administrador",
            Email = "admin@getbud.co",
            Role = EmployeeRole.Leader,
            OrganizationId = org.Id
        };
        dbContext.Employees.Add(adminLeader);

        var team = new Team { Id = Guid.NewGuid(), Name = "getbud.co", OrganizationId = org.Id, LeaderId = adminLeader.Id };
        dbContext.Teams.Add(team);

        await dbContext.SaveChangesAsync();

        adminLeader.TeamId = team.Id;
        await dbContext.SaveChangesAsync();

        return adminLeader.Id;
    }

    private async Task<Employee> CreateNonLeaderEmployee(Guid organizationId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Bud.Infrastructure.Persistence.ApplicationDbContext>();

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Colaborador Teste",
            Email = $"colaborador-{Guid.NewGuid():N}@test.com",
            Role = EmployeeRole.IndividualContributor,
            OrganizationId = organizationId
        };

        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync();

        return employee;
    }

    private async Task<Employee> CreateEmployee(Guid organizationId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Bud.Infrastructure.Persistence.ApplicationDbContext>();

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Colaborador Alvo",
            Email = $"alvo-{Guid.NewGuid():N}@test.com",
            Role = EmployeeRole.IndividualContributor,
            OrganizationId = organizationId
        };

        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync();

        return employee;
    }
}
