namespace Bud.Application.Features.Me;

public sealed class DashboardResource
{
    public static DashboardResource Instance { get; } = new();

    private DashboardResource()
    {
    }
}
