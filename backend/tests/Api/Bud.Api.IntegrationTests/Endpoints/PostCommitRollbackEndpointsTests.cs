using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Bud.Api.IntegrationTests.Helpers;
using Bud.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Bud.Api.IntegrationTests.Endpoints;

public sealed class PostCommitRollbackEndpointsTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task CreateMission_WhenNotificationHandlerFails_RollsBackMissionPersistence()
    {
        using var failingFactory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IDomainEventNotifier<MissionCreatedDomainEvent>>();
                services.AddScoped<IDomainEventNotifier<MissionCreatedDomainEvent>, ThrowingMissionCreatedDomainEventNotifier>();
            }));

        var client = CreateGlobalAdminClient(failingFactory);
        var orgName = $"rollback-mission-{Guid.NewGuid():N}.com";
        var missionName = $"Missão rollback {Guid.NewGuid():N}";

        var orgResponse = await client.PostAsJsonAsync("/api/organizations", new CreateOrganizationRequest
        {
            Name = orgName
        });
        orgResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var organization = (await orgResponse.Content.ReadFromJsonAsync<Organization>())!;
        SetTenantHeader(client, organization.Id);

        var response = await client.PostAsJsonAsync("/api/missions", new CreateMissionRequest
        {
            Name = missionName,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            Status = Bud.Shared.Kernel.Enums.MissionStatus.Planned
        });

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        await using var scope = failingFactory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await dbContext.Missions
            .IgnoreQueryFilters()
            .AnyAsync(m => m.Name == missionName))
            .Should()
            .BeFalse();
    }

    [Fact]
    public async Task CreateCheckin_WhenNotificationHandlerFails_RollsBackCheckinPersistence()
    {
        using var failingFactory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IDomainEventNotifier<CheckinCreatedDomainEvent>>();
                services.AddScoped<IDomainEventNotifier<CheckinCreatedDomainEvent>, ThrowingCheckinCreatedDomainEventNotifier>();
            }));

        var adminClient = CreateGlobalAdminClient(failingFactory);
        var orgName = $"rollback-checkin-{Guid.NewGuid():N}.com";

        var orgResponse = await adminClient.PostAsJsonAsync("/api/organizations", new CreateOrganizationRequest
        {
            Name = orgName
        });
        orgResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var organization = (await orgResponse.Content.ReadFromJsonAsync<Organization>())!;
        SetTenantHeader(adminClient, organization.Id);

        var missionResponse = await adminClient.PostAsJsonAsync("/api/missions", new CreateMissionRequest
        {
            Name = $"Meta ativa {Guid.NewGuid():N}",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30),
            Status = Bud.Shared.Kernel.Enums.MissionStatus.Active
        });
        missionResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var mission = (await missionResponse.Content.ReadFromJsonAsync<Mission>())!;

        var indicatorResponse = await adminClient.PostAsJsonAsync("/api/indicators", new CreateIndicatorRequest
        {
            MissionId = mission.Id,
            Name = $"Indicador rollback {Guid.NewGuid():N}",
            Type = Bud.Shared.Kernel.Enums.IndicatorType.Qualitative,
            TargetText = "Fechar teste de rollback"
        });
        indicatorResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var indicator = (await indicatorResponse.Content.ReadFromJsonAsync<Indicator>())!;

        var employee = await CreateEmployeeInOrganizationAsync(failingFactory, organization.Id);
        var tenantClient = CreateTenantClient(failingFactory, organization.Id, employee.Email, employee.Id);
        var checkinNote = $"nota rollback {Guid.NewGuid():N}";

        var response = await tenantClient.PostAsJsonAsync($"/api/indicators/{indicator.Id}/checkins", new CreateCheckinRequest
        {
            Text = "Check-in que deve sofrer rollback",
            CheckinDate = DateTime.UtcNow,
            ConfidenceLevel = 3,
            Note = checkinNote
        });

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        await using var scope = failingFactory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await dbContext.Checkins
            .IgnoreQueryFilters()
            .AnyAsync(c => c.Note == checkinNote))
            .Should()
            .BeFalse();
    }

    private static void SetTenantHeader(HttpClient client, Guid organizationId)
    {
        client.DefaultRequestHeaders.Remove("X-Tenant-Id");
        client.DefaultRequestHeaders.Add("X-Tenant-Id", organizationId.ToString());
    }

    private static HttpClient CreateGlobalAdminClient(WebApplicationFactory<Program> factory)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JwtTestHelper.GenerateGlobalAdminToken());
        return client;
    }

    private static HttpClient CreateTenantClient(
        WebApplicationFactory<Program> factory,
        Guid organizationId,
        string email,
        Guid employeeId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            JwtTestHelper.GenerateTenantUserToken(email, organizationId, employeeId));
        client.DefaultRequestHeaders.Add("X-Tenant-Id", organizationId.ToString());
        return client;
    }

    private static async Task<Employee> CreateEmployeeInOrganizationAsync(
        WebApplicationFactory<Program> factory,
        Guid organizationId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Colaborador rollback",
            Email = $"rollback-{Guid.NewGuid():N}@test.com",
            Role = EmployeeRole.IndividualContributor,
            OrganizationId = organizationId
        };

        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync();

        return employee;
    }

    private sealed class ThrowingMissionCreatedDomainEventNotifier : IDomainEventNotifier<MissionCreatedDomainEvent>
    {
        public Task HandleAsync(
            MissionCreatedDomainEvent domainEvent,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Falha forçada no handler de missão.");
    }

    private sealed class ThrowingCheckinCreatedDomainEventNotifier : IDomainEventNotifier<CheckinCreatedDomainEvent>
    {
        public Task HandleAsync(
            CheckinCreatedDomainEvent domainEvent,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Falha forçada no handler de check-in.");
    }
}
