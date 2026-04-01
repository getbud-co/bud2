using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bud.Infrastructure.Persistence;
using Bud.Api.IntegrationTests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bud.Api.IntegrationTests.Endpoints;

public sealed class DashboardEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DashboardEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetMyDashboard_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/me/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyDashboard_WithoutEmployeeInToken_ReturnsForbidden()
    {
        var tenantUser = await GetOrCreateTenantUserAsync();
        var client = _factory.CreateClient();
        var token = JwtTestHelper.GenerateTenantUserTokenWithoutEmployee(tenantUser.Email, tenantUser.OrganizationId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantUser.OrganizationId.ToString());

        var response = await client.GetAsync("/api/me/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Detail.Should().Be("Funcionário não identificado.");
    }

    [Fact]
    public async Task GetMyDashboard_WithValidAuthenticatedEmployee_ReturnsOk()
    {
        var tenantUser = await GetOrCreateTenantUserAsync();
        var client = _factory.CreateTenantClient(tenantUser.OrganizationId, tenantUser.Email, tenantUser.EmployeeId);

        var response = await client.GetAsync("/api/me/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMyDashboard_WithUnknownEmployee_ReturnsNotFound()
    {
        var tenantUser = await GetOrCreateTenantUserAsync();
        var client = _factory.CreateTenantClient(tenantUser.OrganizationId, tenantUser.Email, Guid.NewGuid());

        var response = await client.GetAsync("/api/me/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Detail.Should().Be("Funcionário não encontrado.");
    }

    [Fact]
    public async Task GetMyDashboard_WithTeamId_ReturnsOk()
    {
        var tenantUser = await GetOrCreateTenantUserAsync();
        var teamId = await GetOrCreateTeamWithMemberAsync(tenantUser.OrganizationId);
        var client = _factory.CreateTenantClient(tenantUser.OrganizationId, tenantUser.Email, tenantUser.EmployeeId);

        var response = await client.GetAsync($"/api/me/dashboard?teamId={teamId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMyDashboard_WithNonExistentTeamId_ReturnsOkEmpty()
    {
        var tenantUser = await GetOrCreateTenantUserAsync();
        var client = _factory.CreateTenantClient(tenantUser.OrganizationId, tenantUser.Email, tenantUser.EmployeeId);

        var response = await client.GetAsync($"/api/me/dashboard?teamId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<Guid> GetOrCreateTeamWithMemberAsync(Guid organizationId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var teamId = Guid.NewGuid();

        var leader = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Dashboard Filter Leader",
            Email = $"filter-leader-{teamId:N}@test.com",
            Role = EmployeeRole.Leader,
            TeamId = null,
            OrganizationId = organizationId
        };
        dbContext.Employees.Add(leader);
        await dbContext.SaveChangesAsync();

        var team = new Team
        {
            Id = teamId,
            Name = "Dashboard Filter Team",
            OrganizationId = organizationId,
            LeaderId = leader.Id
        };
        dbContext.Teams.Add(team);

        var member = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Team Member",
            Email = $"team-member-{teamId:N}@test.com",
            OrganizationId = organizationId
        };
        dbContext.Employees.Add(member);

        dbContext.Set<EmployeeTeam>().Add(new EmployeeTeam
        {
            EmployeeId = member.Id,
            TeamId = teamId
        });

        await dbContext.SaveChangesAsync();
        return teamId;
    }

    private async Task<(Guid OrganizationId, Guid EmployeeId, string Email)> GetOrCreateTenantUserAsync()
    {
        const string email = "admin@getbud.co";

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var employee = await dbContext.Employees
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Email == email);

        if (employee is not null)
        {
            return (employee.OrganizationId, employee.Id, employee.Email);
        }

        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "dashboard-test.com"
        };
        dbContext.Organizations.Add(org);

        var leader = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Administrador Dashboard",
            Email = email,
            Role = EmployeeRole.Leader,
            TeamId = null,
            OrganizationId = org.Id
        };
        dbContext.Employees.Add(leader);
        await dbContext.SaveChangesAsync();

        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "Time Dashboard",
            OrganizationId = org.Id,
            LeaderId = leader.Id
        };
        dbContext.Teams.Add(team);

        leader.TeamId = team.Id;

        await dbContext.SaveChangesAsync();

        await dbContext.SaveChangesAsync();

        return (org.Id, leader.Id, leader.Email);
    }
}
