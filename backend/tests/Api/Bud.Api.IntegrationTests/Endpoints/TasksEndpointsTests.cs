using System.Net;
using System.Net.Http.Json;
using Bud.Shared.Contracts;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bud.Api.IntegrationTests.Endpoints;

public sealed class TasksEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public TasksEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateGlobalAdminClient();
    }

    [Fact]
    public async Task Create_ShouldReturnCanonicalTaskLocation_AndGetByIdShouldReturnTask()
    {
        await GetOrCreateAdminLeader();

        var orgResponse = await _client.PostAsJsonAsync("/api/organizations",
            new CreateOrganizationRequest
            {
                Name = $"task-org-{Guid.NewGuid():N}.com"
            });
        orgResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var org = (await orgResponse.Content.ReadFromJsonAsync<Organization>())!;
        SetTenantHeader(org.Id);

        var missionResponse = await _client.PostAsJsonAsync("/api/missions",
            new CreateMissionRequest
            {
                Name = "Meta para tarefa",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7),
                Status = Bud.Shared.Kernel.Enums.MissionStatus.Planned
            });
        missionResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var mission = (await missionResponse.Content.ReadFromJsonAsync<Mission>())!;

        var createResponse = await _client.PostAsJsonAsync($"/api/missions/{mission.Id}/tasks",
            new CreateTaskRequest
            {
                Name = "Tarefa criada",
                Description = "Descrição",
                State = Bud.Shared.Kernel.Enums.TaskState.ToDo
            });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskResponse>();
        createdTask.Should().NotBeNull();
        createResponse.Headers.Location.Should().NotBeNull();
        createResponse.Headers.Location!.AbsolutePath.Should().Be($"/api/tasks/{createdTask!.Id}");

        var getResponse = await _client.GetAsync(createResponse.Headers.Location);

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetchedTask = await getResponse.Content.ReadFromJsonAsync<TaskResponse>();
        fetchedTask.Should().NotBeNull();
        fetchedTask!.Id.Should().Be(createdTask.Id);
        fetchedTask.MissionId.Should().Be(mission.Id);
        fetchedTask.Name.Should().Be("Tarefa criada");
    }

    [Fact]
    public async Task GetById_WithUnknownTask_ReturnsNotFound()
    {
        await GetOrCreateAdminLeader();

        var response = await _client.GetAsync($"/api/tasks/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
            var existingMember = await dbContext.OrganizationEmployeeMembers
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

        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "getbud.co",
            OrganizationId = org.Id,
            LeaderId = adminLeader.Id
        };
        dbContext.Teams.Add(team);

        await dbContext.SaveChangesAsync();

        dbContext.OrganizationEmployeeMembers.Add(new OrganizationEmployeeMember
        {
            EmployeeId = adminLeader.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.Leader,
            TeamId = team.Id
        });
        await dbContext.SaveChangesAsync();

        SetTenantHeader(org.Id);
        return adminLeader.Id;
    }
}
