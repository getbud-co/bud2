namespace Bud.Server.Application.ReadModels;

public sealed class DashboardSnapshot
{
    public TeamHealthSnapshot TeamHealth { get; set; } = new();
    public List<PendingTaskSnapshot> PendingTasks { get; set; } = [];
}
