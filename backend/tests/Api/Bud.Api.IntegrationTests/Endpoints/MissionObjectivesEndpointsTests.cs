using System.Net;
using System.Net.Http.Json;
using Bud.Shared.Contracts;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bud.Api.IntegrationTests.Endpoints;

public class MissionObjectivesEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public MissionObjectivesEndpointsTests(CustomWebApplicationFactory factory)
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
            var existingMember = await dbContext.Memberships
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.EmployeeId == existingLeader.Id);
            SetTenantHeader(existingMember!.OrganizationId);
            return existingLeader.Id;
        }

        var org = new Organization { Id = Guid.NewGuid(), Name = "getbud.co" };
        dbContext.Organizations.Add(org);

        var adminLeader = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Administrador",
            Email = "admin@getbud.co",
        };
        dbContext.Employees.Add(adminLeader);

        var team = new Team { Id = Guid.NewGuid(), Name = "getbud.co", OrganizationId = org.Id };
        dbContext.Teams.Add(team);

        dbContext.Memberships.Add(new Membership
        {
            EmployeeId = adminLeader.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.TeamLeader,
            IsGlobalAdmin = true
        });
        dbContext.EmployeeTeams.Add(new EmployeeTeam
        {
            EmployeeId = adminLeader.Id,
            TeamId = team.Id,
            AssignedAt = DateTime.UtcNow,
        });

        await dbContext.SaveChangesAsync();

        SetTenantHeader(org.Id);
        return adminLeader.Id;
    }

    private async Task<Mission> CreateTestMission()
    {
        await GetOrCreateAdminLeader();
        var missionResponse = await _client.PostAsJsonAsync("/api/missions",
            new CreateMissionRequest
            {
                Name = "Test Mission",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7),
                Status = Bud.Shared.Kernel.Enums.MissionStatus.Planned,
            });

        return (await missionResponse.Content.ReadFromJsonAsync<Mission>())!;
    }

    private static CreateMissionRequest ChildMissionRequest(Mission parent, string name, string? description = null, string? dimension = null)
        => new()
        {
            ParentId = parent.Id,
            Name = name,
            Description = description,
            Dimension = dimension,
            StartDate = parent.StartDate,
            EndDate = parent.EndDate,
            Status = Bud.Shared.Kernel.Enums.MissionStatus.Planned,
        };

    #region Create Tests

    [Fact]
    public async Task Create_WithValidData_ReturnsCreated()
    {
        var mission = await CreateTestMission();
        var request = ChildMissionRequest(mission, "Objetivo Estratégico", "Descrição do objetivo", "Clientes");

        var response = await _client.PostAsJsonAsync("/api/missions", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var objective = await response.Content.ReadFromJsonAsync<Mission>();
        objective.Should().NotBeNull();
        objective!.Name.Should().Be("Objetivo Estratégico");
        objective.Description.Should().Be("Descrição do objetivo");
        objective.Dimension.Should().Be("Clientes");
        objective.ParentId.Should().Be(mission.Id);
        objective.OrganizationId.Should().Be(mission.OrganizationId);
    }

    [Fact]
    public async Task Create_WithInvalidParentId_ReturnsNotFound()
    {
        var mission = await CreateTestMission();
        var request = new CreateMissionRequest
        {
            ParentId = Guid.NewGuid(), // Non-existent parent
            Name = "Objetivo",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30),
            Status = Bud.Shared.Kernel.Enums.MissionStatus.Planned,
        };

        var response = await _client.PostAsJsonAsync("/api/missions", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequest()
    {
        var mission = await CreateTestMission();

        var request = ChildMissionRequest(mission, "");

        var response = await _client.PostAsJsonAsync("/api/missions", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithEndDateAfterParent_ReturnsBadRequest()
    {
        var mission = await CreateTestMission();
        var request = ChildMissionRequest(mission, "Objetivo fora da janela");
        request.EndDate = mission.EndDate.AddDays(1);

        var response = await _client.PostAsJsonAsync("/api/missions", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Detail.Should().Contain("data de término");
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidData_ReturnsOk()
    {
        var mission = await CreateTestMission();
        var createResponse = await _client.PostAsJsonAsync("/api/missions", ChildMissionRequest(mission, "Original"));
        var created = await createResponse.Content.ReadFromJsonAsync<Mission>();

        var updateRequest = new PatchMissionRequest
        {
            Name = "Atualizado",
            Description = "Nova descrição",
            Dimension = "Clientes"
        };

        var response = await _client.PatchAsJsonAsync($"/api/missions/{created!.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<Mission>();
        updated!.Name.Should().Be("Atualizado");
        updated.Description.Should().Be("Nova descrição");
        updated.Dimension.Should().Be("Clientes");
    }

    [Fact]
    public async Task Update_WithNonExistentId_ReturnsNotFound()
    {
        var response = await _client.PatchAsJsonAsync($"/api/missions/{Guid.NewGuid()}",
            new PatchMissionRequest { Name = "X" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithEndDateAfterParent_ReturnsBadRequest()
    {
        var mission = await CreateTestMission();
        var createResponse = await _client.PostAsJsonAsync("/api/missions", ChildMissionRequest(mission, "Original"));
        var created = await createResponse.Content.ReadFromJsonAsync<Mission>();

        var response = await _client.PatchAsJsonAsync($"/api/missions/{created!.Id}", new PatchMissionRequest
        {
            EndDate = mission.EndDate.AddDays(1)
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Detail.Should().Contain("data de término");
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithNoChildren_ReturnsNoContent()
    {
        var mission = await CreateTestMission();

        var createResponse = await _client.PostAsJsonAsync("/api/missions", ChildMissionRequest(mission, "Objetivo"));
        var created = await createResponse.Content.ReadFromJsonAsync<Mission>();

        var response = await _client.DeleteAsync($"/api/missions/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync($"/api/missions/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsOk()
    {
        var mission = await CreateTestMission();

        var createResponse = await _client.PostAsJsonAsync("/api/missions", ChildMissionRequest(mission, "Objetivo"));
        var created = await createResponse.Content.ReadFromJsonAsync<Mission>();

        var response = await _client.GetAsync($"/api/missions/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var objective = await response.Content.ReadFromJsonAsync<Mission>();
        objective!.Name.Should().Be("Objetivo");
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/missions/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ReturnsAllChildrenFromParent()
    {
        var mission = await CreateTestMission();

        await _client.PostAsJsonAsync("/api/missions", ChildMissionRequest(mission, "Objetivo A"));
        await _client.PostAsJsonAsync("/api/missions", ChildMissionRequest(mission, "Objetivo B"));

        var response = await _client.GetAsync($"/api/missions/{mission.Id}/children");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Mission>>();
        result!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithInvalidPageSize_ReturnsBadRequest()
    {
        var response = await _client.GetAsync($"/api/missions/{Guid.NewGuid()}/children?page=1&pageSize=101");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Detail.Should().Be("O parâmetro 'pageSize' deve estar entre 1 e 100.");
    }

    #endregion

    #region Convenience Endpoint Tests

    [Fact]
    public async Task GetChildren_ViaMissionsEndpoint_ReturnsOk()
    {
        var mission = await CreateTestMission();

        await _client.PostAsJsonAsync("/api/missions", ChildMissionRequest(mission, "Objetivo via Meta"));

        var response = await _client.GetAsync($"/api/missions/{mission.Id}/children");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Mission>>();
        result!.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("Objetivo via Meta");
    }

    #endregion

    #region Progress Tests

    [Fact]
    public async Task GetProgress_WithValidIds_ReturnsOk()
    {
        var mission = await CreateTestMission();

        var createResponse = await _client.PostAsJsonAsync("/api/missions", ChildMissionRequest(mission, "Objetivo"));
        var objective = await createResponse.Content.ReadFromJsonAsync<Mission>();

        var response = await _client.GetAsync($"/api/missions/progress?ids={objective!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var progress = await response.Content.ReadFromJsonAsync<List<MissionProgressResponse>>();
        progress.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProgress_WithInvalidIds_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/missions/progress?ids=abc");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        var unauthenticatedClient = _factory.CreateClient();

        var response = await unauthenticatedClient.GetAsync($"/api/missions/{Guid.NewGuid()}/children");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_WithTenantMismatch_ReturnsForbidden()
    {
        await GetOrCreateAdminLeader();
        var org1Response = await _client.PostAsJsonAsync("/api/organizations",
            new CreateOrganizationRequest { Name = $"obj-org-1-{Guid.NewGuid():N}.com" });
        var org1 = await org1Response.Content.ReadFromJsonAsync<Organization>();

        var missionResponse = await _client.PostAsJsonAsync("/api/missions",
            new CreateMissionRequest
            {
                Name = "Mission Org 2",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7),
                Status = Bud.Shared.Kernel.Enums.MissionStatus.Planned,
            });
        var mission = await missionResponse.Content.ReadFromJsonAsync<Mission>();

        var employee = await CreateNonOwnerEmployee(org1!.Id);
        var tenantClient = _factory.CreateTenantClient(org1.Id, employee.Email, employee.Id);

        var request = new CreateMissionRequest
        {
            ParentId = mission!.Id,
            Name = "Objetivo Proibido",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30),
            Status = Bud.Shared.Kernel.Enums.MissionStatus.Planned,
        };

        var response = await tenantClient.PostAsJsonAsync("/api/missions", request);

        // NotFound instead of Forbidden: tenant isolation hides cross-org resources
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Indicator with Child Mission Tests

    [Fact]
    public async Task CreateIndicator_WithChildMissionId_AssociatesCorrectly()
    {
        var mission = await CreateTestMission();

        var objectiveResponse = await _client.PostAsJsonAsync("/api/missions", ChildMissionRequest(mission, "Objetivo"));
        var objective = await objectiveResponse.Content.ReadFromJsonAsync<Mission>();

        var indicatorResponse = await _client.PostAsJsonAsync("/api/indicators",
            new CreateIndicatorRequest
            {
                MissionId = objective!.Id,
                Name = "Métrica do Objetivo",
                Type = Bud.Shared.Kernel.Enums.IndicatorType.Qualitative,
                TargetText = "Teste"
            });

        indicatorResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var indicator = await indicatorResponse.Content.ReadFromJsonAsync<Indicator>();
        indicator!.MissionId.Should().Be(objective.Id);
    }

    [Fact]
    public async Task GetIndicators_FilteredByChildMission_ReturnsCorrectIndicators()
    {
        var mission = await CreateTestMission();

        var objectiveResponse = await _client.PostAsJsonAsync("/api/missions", ChildMissionRequest(mission, "Objetivo"));
        var objective = await objectiveResponse.Content.ReadFromJsonAsync<Mission>();

        await _client.PostAsJsonAsync("/api/indicators",
            new CreateIndicatorRequest
            {
                MissionId = objective!.Id,
                Name = "Métrica do Objetivo",
                Type = Bud.Shared.Kernel.Enums.IndicatorType.Qualitative,
                TargetText = "T1"
            });

        await _client.PostAsJsonAsync("/api/indicators",
            new CreateIndicatorRequest
            {
                MissionId = mission.Id,
                Name = "Métrica Direta",
                Type = Bud.Shared.Kernel.Enums.IndicatorType.Qualitative,
                TargetText = "T2"
            });

        var response = await _client.GetAsync($"/api/indicators?missionId={objective.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Indicator>>();
        result!.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("Métrica do Objetivo");
    }

    #endregion

    private async Task<Employee> CreateNonOwnerEmployee(Guid organizationId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Bud.Infrastructure.Persistence.ApplicationDbContext>();

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Colaborador Teste",
            Email = $"colaborador-{Guid.NewGuid():N}@test.com",
        };

        dbContext.Employees.Add(employee);

        dbContext.Memberships.Add(new Membership
        {
            EmployeeId = employee.Id,
            OrganizationId = organizationId,
            Role = EmployeeRole.Contributor,
        });

        await dbContext.SaveChangesAsync();

        return employee;
    }
}
