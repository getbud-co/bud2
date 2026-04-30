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

        services.AddScoped<CreateTeam>();
        services.AddScoped<PatchTeam>();
        services.AddScoped<DeleteTeam>();
        services.AddScoped<GetTeamById>();
        services.AddScoped<ListTeams>();
        services.AddScoped<ListSubTeams>();
        services.AddScoped<ListTeamEmployees>();
        services.AddScoped<GetTeamEmployeeLookup>();
        services.AddScoped<PatchTeamEmployees>();
        services.AddScoped<ListAvailableEmployeesForTeam>();
        services.AddScoped<BulkArchiveTeams>();
        services.AddScoped<BulkDeleteTeams>();

        services.AddScoped<CreateEmployee>();
        services.AddScoped<PatchEmployee>();
        services.AddScoped<DeleteEmployee>();
        services.AddScoped<GetEmployeeById>();
        services.AddScoped<ListLeaderEmployees>();
        services.AddScoped<ListEmployees>();
        services.AddScoped<GetEmployeeHierarchy>();
        services.AddScoped<ListEmployeeTeams>();
        services.AddScoped<PatchEmployeeTeams>();
        services.AddScoped<ListAvailableTeamsForEmployee>();
        services.AddScoped<GetEmployeeLookup>();

        services.AddScoped<CreateCycle>();
        services.AddScoped<PatchCycle>();
        services.AddScoped<DeleteCycle>();
        services.AddScoped<GetCycleById>();
        services.AddScoped<ListCycles>();

        services.AddScoped<CreateSession>();
        services.AddScoped<ListMyOrganizations>();
        services.AddScoped<DeleteCurrentSession>();

        services.AddScoped<NotificationOrchestrator>();
        services.AddScoped<ListNotifications>();
        services.AddScoped<PatchNotification>();
        services.AddScoped<PatchNotifications>();

        return services;
    }
}
