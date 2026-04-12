using System.Net;
using System.Net.Http.Json;
using Bud.Shared.Contracts;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bud.Api.IntegrationTests.Endpoints;

public class OrganizationsEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public OrganizationsEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateGlobalAdminClient();
    }

    private async Task<Guid> GetOrCreateAdminLeader()
    {
        // Create bootstrap hierarchy similar to DbSeeder
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Bud.Infrastructure.Persistence.ApplicationDbContext>();

        // Check if admin leader already exists (ignore query filters like DbSeeder does)
        var existingLeader = await dbContext.Employees
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Email == "admin@getbud.co");

        if (existingLeader != null)
        {
            // Ensure org and team exist
            var existingOrg = await dbContext.Organizations.IgnoreQueryFilters().FirstOrDefaultAsync();
            if (existingOrg == null)
            {
                existingOrg = new Organization { Id = Guid.NewGuid(), Name = "getbud.co" };
                dbContext.Organizations.Add(existingOrg);
            }

            var existingTeam = await dbContext.Teams.IgnoreQueryFilters().FirstOrDefaultAsync();
            if (existingTeam == null)
            {
                existingTeam = new Team { Id = Guid.NewGuid(), Name = "Bud", OrganizationId = existingOrg.Id };
                dbContext.Teams.Add(existingTeam);
            }

            await dbContext.SaveChangesAsync();
            return existingLeader.Id;
        }

        // Create hierarchy: Org -> Team -> Leader
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "getbud.co"
        };
        dbContext.Organizations.Add(org);

        var adminLeader = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Administrador",
            Email = "admin@getbud.co",
        };
        dbContext.Employees.Add(adminLeader);
        await dbContext.SaveChangesAsync();

        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "Bud",
            OrganizationId = org.Id
        };
        dbContext.Teams.Add(team);
        await dbContext.SaveChangesAsync();

        dbContext.OrganizationEmployeeMembers.Add(new OrganizationEmployeeMember
        {
            EmployeeId = adminLeader.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.TeamLeader,
            TeamId = team.Id
        });
        await dbContext.SaveChangesAsync();

        return adminLeader.Id;
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        var organizationDomain = $"test-org-{Guid.NewGuid():N}.com";
        var request = new CreateOrganizationRequest
        {
            Name = organizationDomain
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/organizations", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var organization = await response.Content.ReadFromJsonAsync<Organization>();
        organization.Should().NotBeNull();
        organization!.Name.Should().Be(organizationDomain);
        organization.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateOrganizationRequest
        {
            Name = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/organizations", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithDuplicateDomain_ReturnsConflict()
    {
        var request = new CreateOrganizationRequest
        {
            Name = $"duplicate-{Guid.NewGuid():N}.com"
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/organizations", request);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var secondResponse = await _client.PostAsJsonAsync("/api/organizations", request);

        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await secondResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Detail.Should().Be("Já existe uma organização cadastrada com este domínio.");
    }

    [Fact]
    public async Task GetById_WithExistingId_ReturnsOk()
    {
        // Arrange
        var organizationDomain = $"getbyid-test-{Guid.NewGuid():N}.com";
        var createRequest = new CreateOrganizationRequest
        {
            Name = organizationDomain
        };
        var createResponse = await _client.PostAsJsonAsync("/api/organizations", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<Organization>();

        // Act
        var response = await _client.GetAsync($"/api/organizations/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var organization = await response.Content.ReadFromJsonAsync<Organization>();
        organization.Should().NotBeNull();
        organization!.Id.Should().Be(created.Id);
        organization.Name.Should().Be(organizationDomain);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/organizations/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_ReturnsPagedResult()
    {
        // Arrange
        var firstDomain = $"org1-{Guid.NewGuid():N}.com";
        var secondDomain = $"org2-{Guid.NewGuid():N}.com";
        await _client.PostAsJsonAsync("/api/organizations", new CreateOrganizationRequest
        {
            Name = firstDomain
        });
        await _client.PostAsJsonAsync("/api/organizations", new CreateOrganizationRequest
        {
            Name = secondDomain
        });

        // Act
        var response = await _client.GetAsync("/api/organizations?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Organization>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetAll_WithSearchTooLong_ReturnsBadRequest()
    {
        // Arrange
        var search = new string('a', 201);

        // Act
        var response = await _client.GetAsync($"/api/organizations?search={search}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Detail.Should().Be("O parâmetro 'search' deve ter no máximo 200 caracteres.");
    }

    [Fact]
    public async Task GetAll_WithInvalidPage_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/organizations?page=0&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Detail.Should().Be("O parâmetro 'page' deve ser maior ou igual a 1.");
    }

    [Fact]
    public async Task GetAll_WithInvalidPageSize_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/organizations?page=1&pageSize=101");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Detail.Should().Be("O parâmetro 'pageSize' deve estar entre 1 e 100.");
    }

    [Fact]
    public async Task Update_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var originalDomain = $"original-{Guid.NewGuid():N}.com";
        var updatedDomain = $"updated-{Guid.NewGuid():N}.com";
        var createRequest = new CreateOrganizationRequest
        {
            Name = originalDomain
        };
        var createResponse = await _client.PostAsJsonAsync("/api/organizations", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<Organization>();

        var updateRequest = new PatchOrganizationRequest { Name = updatedDomain };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/organizations/{created!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<Organization>();
        updated.Should().NotBeNull();
        updated!.Name.Should().Be(updatedDomain);
    }

    [Fact]
    public async Task Update_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var updateRequest = new PatchOrganizationRequest { Name = "updated.com" };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/organizations/{nonExistingId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithExistingId_ReturnsNoContent()
    {
        // Arrange
        var createRequest = new CreateOrganizationRequest
        {
            Name = "to-delete.com"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/organizations", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<Organization>();

        // Act
        var response = await _client.DeleteAsync($"/api/organizations/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's deleted
        var getResponse = await _client.GetAsync($"/api/organizations/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/organizations/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithNonAdminUser_ReturnsForbidden()
    {
        // Arrange
        await GetOrCreateAdminLeader();

        // Create a non-admin employee for testing
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Bud.Infrastructure.Persistence.ApplicationDbContext>();

        var org = await dbContext.Organizations.IgnoreQueryFilters().FirstAsync();
        var team = await dbContext.Teams.IgnoreQueryFilters().FirstAsync();

        var nonAdminEmployee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Usuário Regular",
            Email = $"user-create-{Guid.NewGuid()}@test.com",
        };
        dbContext.Employees.Add(nonAdminEmployee);
        await dbContext.SaveChangesAsync();

        dbContext.OrganizationEmployeeMembers.Add(new OrganizationEmployeeMember
        {
            EmployeeId = nonAdminEmployee.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.Contributor,
            TeamId = team.Id
        });
        await dbContext.SaveChangesAsync();

        var nonAdminClient = _factory.CreateTenantClient(
            org.Id,
            nonAdminEmployee.Email,
            nonAdminEmployee.Id);

        var request = new CreateOrganizationRequest
        {
            Name = "unauthorized-org.com"
        };

        // Act
        var response = await nonAdminClient.PostAsJsonAsync("/api/organizations", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_WithNonAdminUser_ReturnsForbidden()
    {
        // Arrange
        await GetOrCreateAdminLeader();

        // Create a non-admin employee
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Bud.Infrastructure.Persistence.ApplicationDbContext>();

        var org = await dbContext.Organizations.IgnoreQueryFilters().FirstAsync();
        var team = await dbContext.Teams.IgnoreQueryFilters().FirstAsync();

        var nonAdminEmployee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Usuário Regular",
            Email = $"user-update-{Guid.NewGuid()}@test.com",
        };
        dbContext.Employees.Add(nonAdminEmployee);
        await dbContext.SaveChangesAsync();

        dbContext.OrganizationEmployeeMembers.Add(new OrganizationEmployeeMember
        {
            EmployeeId = nonAdminEmployee.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.Contributor,
            TeamId = team.Id
        });
        await dbContext.SaveChangesAsync();

        var nonAdminClient = _factory.CreateTenantClient(
            org.Id,
            nonAdminEmployee.Email,
            nonAdminEmployee.Id);

        var request = new PatchOrganizationRequest { Name = "updated.com" };

        // Act
        var response = await nonAdminClient.PatchAsJsonAsync($"/api/organizations/{org.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_WithNonAdminUser_ReturnsForbidden()
    {
        // Arrange
        await GetOrCreateAdminLeader();

        // Create a non-admin employee
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Bud.Infrastructure.Persistence.ApplicationDbContext>();

        var org = await dbContext.Organizations.IgnoreQueryFilters().FirstAsync();
        var team = await dbContext.Teams.IgnoreQueryFilters().FirstAsync();

        var nonAdminEmployee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Usuário Regular",
            Email = $"user-delete-{Guid.NewGuid()}@test.com",
        };
        dbContext.Employees.Add(nonAdminEmployee);
        await dbContext.SaveChangesAsync();

        dbContext.OrganizationEmployeeMembers.Add(new OrganizationEmployeeMember
        {
            EmployeeId = nonAdminEmployee.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.Contributor,
            TeamId = team.Id
        });
        await dbContext.SaveChangesAsync();

        var nonAdminClient = _factory.CreateTenantClient(
            org.Id,
            nonAdminEmployee.Email,
            nonAdminEmployee.Id);

        // Act
        var response = await nonAdminClient.DeleteAsync($"/api/organizations/{org.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        var unauthenticatedClient = _factory.CreateClient();

        var response = await unauthenticatedClient.GetAsync("/api/organizations");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
