using Bud.Application.Abstractions;
using Bud.Application.Features.Me;
using Bud.Application.Features.Organizations;
using Bud.Infrastructure.Persistence;
using Bud.Infrastructure.DomainEvents;
using Bud.Infrastructure.Features.Notifications;
using Bud.Infrastructure.Features.Employees;
using Bud.Infrastructure.Features.Teams;
using Bud.Application.Ports;
using Bud.Infrastructure.Features.Employees;
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

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<ICycleRepository, CycleRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<ISessionAuthenticator, SessionAuthenticator>();
        services.AddScoped<ITenantAuthorizationService, TenantAuthorizationService>();
        services.AddScoped<IMyOrganizationsReadStore, MyOrganizationsReadStore>();
        services.AddScoped<ITenantAuthorizationService, TenantAuthorizationService>();
        services.AddScoped<TenantSaveChangesInterceptor>();

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
