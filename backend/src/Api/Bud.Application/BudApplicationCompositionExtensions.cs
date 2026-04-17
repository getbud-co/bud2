using Microsoft.Extensions.DependencyInjection;

namespace Bud.Application;

public static class BudApplicationCompositionExtensions
{
    public static IServiceCollection AddBudApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateEmployee>();
        services.AddScoped<UpdateEmployee>();
        services.AddScoped<DeleteEmployee>();
        services.AddScoped<GetEmployeeById>();
        services.AddScoped<ListEmployees>();

        services.AddScoped<ListNotifications>();
        services.AddScoped<UpdateNotification>();
        services.AddScoped<UpdateAllNotifications>();
        services.AddScoped<CreateNotifications>();

        services.AddScoped<CreateOrganization>();
        services.AddScoped<UpdateOrganization>();
        services.AddScoped<DeleteOrganization>();
        services.AddScoped<GetOrganizationById>();
        services.AddScoped<ListOrganizations>();

        services.AddScoped<CreateSession>();
        services.AddScoped<ListMyOrganizations>();
        services.AddScoped<DeleteCurrentSession>();

        return services;
    }
}
