using System.Net;
using System.Net.Http.Json;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Bud.Shared.Contracts;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bud.Api.IntegrationTests.Endpoints;

public class MissionsEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public MissionsEndpointsTests(CustomWebApplicationFactory factory)
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
            var existingMember = await dbContext.Memberships.IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.EmployeeId == existingLeader.Id);
            SetTenantHeader(existingMember!.OrganizationId);
            return existingLeader.Id;
        }

        var org = new Organization { Id = Guid.NewGuid(), Name = "getbud.co" };
        dbContext.Organizations.Add(org);

        var adminLeader = new Employee { Id = Guid.NewGuid(), FullName = "Administrador", Email = "admin@getbud.co" };
        dbContext.Employees.Add(adminLeader);

        var team = new Team { Id = Guid.NewGuid(), Name = "getbud.co", OrganizationId = org.Id };
        dbContext.Teams.Add(team);

        dbContext.Memberships.Add(new Membership
        {
            EmployeeId = adminLeader.Id, OrganizationId = org.Id,
            Role = EmployeeRole.TeamLeader, IsGlobalAdmin = true
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

    #region Create Tests

    [Fact]
    public async Task GetAll_ShouldRespondWithinBudget()
    {
        var watch = Stopwatch.StartNew();

        var response = await _client.GetAsync("/api/missions?page=1&pageSize=20");

        watch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        watch.ElapsedMilliseconds.Should().BeLessThan(5000);
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreated()
    {
        // Arrange: Create organization first
        await GetOrCreateAdminLeader();
        var orgResponse = await _client.PostAsJsonAsync("/api/organizations",
            new CreateOrganizationRequest
            {
                Name = "test-mission.com"
            });
        var org = await orgResponse.Content.ReadFromJsonAsync<Organization>();
        SetTenantHeader(org!.Id);

        var request = new CreateMissionRequest
        {
            Name = "Test Mission",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            Status = Bud.Shared.Kernel.Enums.MissionStatus.Planned,
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/missions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var mission = await response.Content.ReadFromJsonAsync<Mission>();
        mission.Should().NotBeNull();
        mission!.Name.Should().Be("Test Mission");
        mission.OrganizationId.Should().Be(org.Id);
        mission.EmployeeId.Should().BeNull();
    }

    [Fact]
    public async Task Create_WithTenantMismatch_ReturnsForbidden()
    {
        // Arrange: create two organizations
        await GetOrCreateAdminLeader();
        var org1Response = await _client.PostAsJsonAsync("/api/organizations",
            new CreateOrganizationRequest
            {
                Name = "org-1.com"
            });
        var org1 = await org1Response.Content.ReadFromJsonAsync<Organization>();

        var org2Response = await _client.PostAsJsonAsync("/api/organizations",
            new CreateOrganizationRequest
            {
                Name = "org-2.com"
            });
        var org2 = await org2Response.Content.ReadFromJsonAsync<Organization>();

        var employee = await CreateNonOwnerEmployee(org1!.Id);
        var tenantClient = _factory.CreateTenantClient(org1.Id, employee.Email, employee.Id);

        // Tenant client is scoped to org1, but tries to create mission referencing org2 employee
        var org2Employee = await CreateNonOwnerEmployee(org2!.Id);
        var request = new CreateMissionRequest
        {
            Name = "Mission Forbidden",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            Status = Bud.Shared.Kernel.Enums.MissionStatus.Planned,
            EmployeeId = org2Employee.Id
        };

        // Act
        var response = await tenantClient.PostAsJsonAsync("/api/missions", request);

        // Assert: NotFound instead of Forbidden — tenant isolation hides cross-org employees
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithInvalidEmployeeId_ReturnsNotFound()
    {
        // Arrange
        await GetOrCreateAdminLeader();
        var request = new CreateMissionRequest
        {
            Name = "Test Mission",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            Status = Bud.Shared.Kernel.Enums.MissionStatus.Planned,
            EmployeeId = Guid.NewGuid() // Non-existent ID
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/missions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithEndDateBeforeStartDate_ReturnsBadRequest()
    {
        // Arrange
        await GetOrCreateAdminLeader();
        var request = new CreateMissionRequest
        {
            Name = "Test Mission",
            StartDate = DateTime.UtcNow.AddDays(7),
            EndDate = DateTime.UtcNow, // Before start date
            Status = Bud.Shared.Kernel.Enums.MissionStatus.Planned,
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/missions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithGlobalAdminAndInvalidTenantHeader_ReturnsForbidden()
    {
        _client.DefaultRequestHeaders.Remove("X-Tenant-Id");
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", Guid.NewGuid().ToString());

        var response = await _client.PostAsJsonAsync("/api/missions", new CreateMissionRequest
        {
            Name = "Mission Invalid Tenant",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            Status = Bud.Shared.Kernel.Enums.MissionStatus.Planned
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Detail.Should().Be("Você não tem permissão para acessar esta organização.");
    }

    [Fact]
    public async Task Create_WithStringEnums_ReturnsCreated()
    {
        await GetOrCreateAdminLeader();
        var payload = new
        {
            name = "Mission String Enum",
            description = "String enum parsing",
            startDate = DateTime.UtcNow,
            endDate = DateTime.UtcNow.AddDays(5),
            status = "Planned"
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/missions", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_WithNumericEnums_ReturnsCreated()
    {
        await GetOrCreateAdminLeader();
        var payload = new
        {
            name = "Mission Numeric Enum",
            description = "Numeric enum parsing",
            startDate = DateTime.UtcNow,
            endDate = DateTime.UtcNow.AddDays(5),
            status = 0
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/missions", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
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
            Email = $"colaborador-{Guid.NewGuid():N}@test.com"
        };

        dbContext.Employees.Add(employee);

        dbContext.Memberships.Add(new Membership
        {
            EmployeeId = employee.Id, OrganizationId = organizationId,
            Role = EmployeeRole.Contributor
        });

        await dbContext.SaveChangesAsync();

        return employee;
    }

    #region GetById Tests

    [Fact]
    public async Task GetById_WithExistingId_ReturnsOk()
    {
        await GetOrCreateAdminLeader();
        // Arrange: Create mission
        var createRequest = new CreateMissionRequest
        {
            Name = "Test Mission for GetById",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            Status = Bud.Shared.Kernel.Enums.MissionStatus.Planned,
        };
        var createResponse = await _client.PostAsJsonAsync("/api/missions", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<Mission>();

        // Act
        var response = await _client.GetAsync($"/api/missions/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var mission = await response.Content.ReadFromJsonAsync<Mission>();
        mission.Should().NotBeNull();
        mission!.Id.Should().Be(created.Id);
        mission.Name.Should().Be("Test Mission for GetById");
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/missions/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithFilter_ReturnsFilteredResults()
    {
        await GetOrCreateAdminLeader();
        // Arrange: Create missions
        await _client.PostAsJsonAsync("/api/missions", new CreateMissionRequest
        {
            Name = "Mission Filter Test",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            Status = Bud.Shared.Kernel.Enums.MissionStatus.Planned,
        });

        // Act - Filter with All
        var response = await _client.GetAsync($"/api/missions?filter={MissionFilter.All}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Mission>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAll_WithPagination_ReturnsPagedResults()
    {
        await GetOrCreateAdminLeader();
        // Arrange: Create multiple missions
        for (int i = 1; i <= 15; i++)
        {
            await _client.PostAsJsonAsync("/api/missions", new CreateMissionRequest
            {
                Name = $"Mission {i}",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7),
                Status = Bud.Shared.Kernel.Enums.MissionStatus.Planned,
            });
        }

        // Act
        var response = await _client.GetAsync("/api/missions?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Mission>>();
        result.Should().NotBeNull();
        result!.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.Should().HaveCountLessOrEqualTo(10);
    }

    [Fact]
    public async Task GetAll_WithInvalidPageSize_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/missions?page=1&pageSize=101");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Detail.Should().Be("O parâmetro 'pageSize' deve estar entre 1 e 100.");
    }

    [Fact]
    public async Task GetAll_WithSearchTooLong_ReturnsBadRequest()
    {
        // Arrange
        var search = new string('a', 201);

        // Act
        var response = await _client.GetAsync($"/api/missions?search={search}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Detail.Should().Be("O parâmetro 'search' deve ter no máximo 200 caracteres.");
    }

    [Fact]
    public async Task GetProgress_WithInvalidIds_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/missions/progress?ids=abc");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Detail.Should().Be("O parâmetro 'ids' contém valores inválidos. Informe GUIDs separados por vírgula.");
    }

    #endregion

    #region GetMyMissions Tests

    [Fact]
    public async Task GetMyMissions_WithValidEmployee_ReturnsHierarchyMissions()
    {
        // Arrange: Create full hierarchy
        await GetOrCreateAdminLeader();
        var organizationDomain = $"test-org-{Guid.NewGuid():N}.com";
        var orgResponse = await _client.PostAsJsonAsync("/api/organizations",
            new CreateOrganizationRequest { Name = organizationDomain });
        var org = await orgResponse.Content.ReadFromJsonAsync<Organization>();
        SetTenantHeader(org!.Id);

        // Create a leader in the new org for team creation
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Bud.Infrastructure.Persistence.ApplicationDbContext>();

        var orgLeader = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Líder Org Teste",
            Email = $"leader-{Guid.NewGuid():N}@{organizationDomain}"
        };
        dbContext.Employees.Add(orgLeader);

        dbContext.Memberships.Add(new Membership
        {
            EmployeeId = orgLeader.Id, OrganizationId = org!.Id,
            Role = EmployeeRole.TeamLeader
        });

        await dbContext.SaveChangesAsync();

        var teamResponse = await _client.PostAsJsonAsync("/api/teams",
            new CreateTeamRequest { Name = "Test Team", LeaderId = orgLeader.Id });
        var team = await teamResponse.Content.ReadFromJsonAsync<Team>();

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Test User",
            Email = $"test-{Guid.NewGuid():N}@example.com"
        };

        dbContext.Employees.Add(employee);

        dbContext.Memberships.Add(new Membership
        {
            EmployeeId = employee.Id, OrganizationId = org!.Id,
            Role = EmployeeRole.Contributor
        });
        dbContext.EmployeeTeams.Add(new EmployeeTeam
        {
            EmployeeId = employee.Id,
            TeamId = team!.Id,
            AssignedAt = DateTime.UtcNow,
        });

        await dbContext.SaveChangesAsync();

        // Create missions: one org-level, one employee-level
        await _client.PostAsJsonAsync("/api/missions", new CreateMissionRequest
        {
            Name = "Org Mission",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            Status = Bud.Shared.Kernel.Enums.MissionStatus.Planned,
        });

        await _client.PostAsJsonAsync("/api/missions", new CreateMissionRequest
        {
            Name = "Employee Mission",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            Status = Bud.Shared.Kernel.Enums.MissionStatus.Planned,
            EmployeeId = employee!.Id
        });

        // Act
        var employeeClient = _factory.CreateTenantClient(org.Id, employee.Email, employee.Id);
        var response = await employeeClient.GetAsync("/api/missions?filter=Mine");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Mission>>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCountGreaterOrEqualTo(1);
        result.Items.Should().Contain(m => m.Name == "Employee Mission");
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidRequest_ReturnsOk()
    {
        await GetOrCreateAdminLeader();
        // Arrange: Create mission
        var createRequest = new CreateMissionRequest
        {
            Name = "Original Name",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            Status = Bud.Shared.Kernel.Enums.MissionStatus.Planned,
        };
        var createResponse = await _client.PostAsJsonAsync("/api/missions", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<Mission>();

        var updateRequest = new PatchMissionRequest
        {
            Name = "Updated Name",
            StartDate = created!.StartDate,
            EndDate = created.EndDate,
            Status = Bud.Shared.Kernel.Enums.MissionStatus.Active
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/missions/{created.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<Mission>();
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Name");
        updated.Status.Should().Be(MissionStatus.Active);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithExistingId_ReturnsNoContent()
    {
        await GetOrCreateAdminLeader();
        // Arrange: Create mission
        var createRequest = new CreateMissionRequest
        {
            Name = "Mission to Delete",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            Status = Bud.Shared.Kernel.Enums.MissionStatus.Planned,
        };
        var createResponse = await _client.PostAsJsonAsync("/api/missions", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<Mission>();

        // Act
        var response = await _client.DeleteAsync($"/api/missions/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's deleted
        var getResponse = await _client.GetAsync($"/api/missions/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var unauthenticatedClient = _factory.CreateClient();

        // Act
        var response = await unauthenticatedClient.GetAsync("/api/missions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
