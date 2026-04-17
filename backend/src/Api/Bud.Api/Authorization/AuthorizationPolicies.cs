namespace Bud.Api.Authorization;

public static class AuthorizationPolicies
{
    public const string TenantSelected = "TenantSelected";
    public const string GlobalAdmin = "GlobalAdmin";
    public const string LeaderRequired = "LeaderRequired";
    public const string HRManagerRequired = "HRManagerRequired";
    public const string MissionOwnerOrHRManager = "MissionOwnerOrHRManager";
    public const string MissionOwnerOrTeamLeader = "MissionOwnerOrTeamLeader";
}
