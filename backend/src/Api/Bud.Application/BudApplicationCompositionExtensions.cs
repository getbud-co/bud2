using Bud.Application.EventHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace Bud.Application;

public static class BudApplicationCompositionExtensions
{
    public static IServiceCollection AddBudApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateOrganization>();
        services.AddScoped<PatchOrganization>();
        services.AddScoped<DeleteOrganization>();
        services.AddScoped<GetOrganizationById>();
        services.AddScoped<ListOrganizations>();
        services.AddScoped<ListOrganizationEmployees>();

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

        services.AddScoped<CreateMission>();
        services.AddScoped<PatchMission>();
        services.AddScoped<DeleteMission>();
        services.AddScoped<GetMissionById>();
        services.AddScoped<ListMissions>();
        services.AddScoped<ListMissionChildren>();
        services.AddScoped<ListMissionIndicators>();
        services.AddScoped<ListMissionProgress>();

        services.AddScoped<CreateTask>();
        services.AddScoped<GetTaskById>();
        services.AddScoped<PatchTask>();
        services.AddScoped<DeleteTask>();
        services.AddScoped<ListTasks>();

        services.AddScoped<CreateIndicator>();
        services.AddScoped<PatchIndicator>();
        services.AddScoped<DeleteIndicator>();
        services.AddScoped<GetIndicatorById>();
        services.AddScoped<ListIndicators>();
        services.AddScoped<GetIndicatorProgress>();

        services.AddScoped<CreateCheckin>();
        services.AddScoped<PatchCheckin>();
        services.AddScoped<DeleteCheckin>();
        services.AddScoped<GetCheckinById>();
        services.AddScoped<ListCheckins>();

        services.AddScoped<CreateSession>();
        services.AddScoped<ListMyOrganizations>();
        services.AddScoped<DeleteCurrentSession>();

        services.AddScoped<CreateTemplate>();
        services.AddScoped<PatchTemplate>();
        services.AddScoped<DeleteTemplate>();
        services.AddScoped<GetTemplateById>();
        services.AddScoped<ListTemplates>();

        services.AddScoped<GetMyDashboard>();

        services.AddScoped<NotificationOrchestrator>();
        services.AddScoped<IDomainEventNotifier<MissionCreatedDomainEvent>, MissionCreatedDomainEventNotifier>();
        services.AddScoped<IDomainEventNotifier<MissionUpdatedDomainEvent>, MissionUpdatedDomainEventNotifier>();
        services.AddScoped<IDomainEventNotifier<MissionDeletedDomainEvent>, MissionDeletedDomainEventNotifier>();
        services.AddScoped<IDomainEventNotifier<CheckinCreatedDomainEvent>, CheckinCreatedDomainEventNotifier>();
        services.AddScoped<ListNotifications>();
        services.AddScoped<PatchNotification>();
        services.AddScoped<PatchNotifications>();

        return services;
    }
}
