using Bud.Application.Abstractions;
using Bud.Application.Features.Missions;
using Bud.Application.Features.Indicators;
using Bud.Application.Features.Tasks;
using Bud.Application.Features.Me;
using Bud.Application.Features.Notifications;
using Bud.Application.Features.Organizations;
using Bud.Application.Features.Templates;
using Bud.Infrastructure.Persistence;
using Bud.Infrastructure.Authorization;
using Bud.Infrastructure.DomainEvents;
using Bud.Infrastructure.Features.Missions;
using Bud.Infrastructure.Features.Indicators;
using Bud.Infrastructure.Features.Notifications;
using Bud.Infrastructure.Features.Tasks;
using Bud.Infrastructure.Features.Templates;
using Bud.Infrastructure.Features.Employees;
using Bud.Infrastructure.Features.Teams;
using Bud.Application.Ports;
using Bud.Infrastructure.Features.Me;
using Bud.Infrastructure.Features.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure;

public static class BudInfrastructureCompositionExtensions
{
    public static IServiceCollection AddBudInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not configured.");
        }

        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<IMissionRepository, MissionRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IIndicatorRepository, IndicatorRepository>();
        services.AddScoped<ITemplateRepository, TemplateRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ISessionAuthenticator, SessionAuthenticator>();
        services.AddScoped<IMyOrganizationsReadStore, MyOrganizationsReadStore>();
        services.AddScoped<IMyDashboardReadStore, DashboardReadStore>();
        services.AddScoped<IMissionProgressReadStore, MissionProgressService>();
        services.AddScoped<IIndicatorProgressReadStore, MissionProgressService>();
        services.AddScoped<INotificationRecipientResolver, NotificationRecipientResolver>();
        services.AddScoped<IReadAuthorizationRule<EmployeeResource>, EmployeeAuthorizationService>();
        services.AddScoped<IWriteAuthorizationRule<EmployeeResource>, EmployeeAuthorizationService>();
        services.AddScoped<IWriteAuthorizationRule<CreateEmployeeContext>, EmployeeAuthorizationService>();
        services.AddScoped<IReadAuthorizationRule<TeamResource>, TeamAuthorizationService>();
        services.AddScoped<IWriteAuthorizationRule<TeamResource>, TeamAuthorizationService>();
        services.AddScoped<IWriteAuthorizationRule<CreateTeamContext>, TeamAuthorizationService>();
        services.AddScoped<IReadAuthorizationRule<MissionResource>, MissionAuthorizationService>();
        services.AddScoped<IWriteAuthorizationRule<MissionResource>, MissionAuthorizationService>();
        services.AddScoped<IWriteAuthorizationRule<CreateMissionContext>, MissionAuthorizationService>();
        services.AddScoped<IReadAuthorizationRule<IndicatorResource>, IndicatorAuthorizationService>();
        services.AddScoped<IWriteAuthorizationRule<IndicatorResource>, IndicatorAuthorizationService>();
        services.AddScoped<IWriteAuthorizationRule<TaskResource>, TaskAuthorizationService>();
        services.AddScoped<IReadAuthorizationRule<TemplateResource>, TemplateAuthorizationService>();
        services.AddScoped<IWriteAuthorizationRule<TemplateResource>, TemplateAuthorizationService>();
        services.AddScoped<IWriteAuthorizationRule<CreateTemplateContext>, TemplateAuthorizationService>();
        services.AddScoped<IReadAuthorizationRule<NotificationInboxResource>, NotificationAuthorizationService>();
        services.AddScoped<IWriteAuthorizationRule<NotificationResource>, NotificationAuthorizationService>();
        services.AddScoped<IReadAuthorizationRule<DashboardResource>, DashboardAuthorizationService>();
        services.AddScoped<ITenantAuthorizationService, TenantAuthorizationService>();
        services.AddScoped<TenantSaveChangesInterceptor>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddScoped<ApplicationDbContext>(sp =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            optionsBuilder.AddInterceptors(sp.GetRequiredService<TenantSaveChangesInterceptor>());

            var tenantProvider = sp.GetRequiredService<ITenantProvider>();
            return new ApplicationDbContext(optionsBuilder.Options, tenantProvider);
        });
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgres", tags: ["db", "ready"]);

        return services;
    }
}
