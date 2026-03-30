
namespace Bud.Application.Features.Tasks;

public static class TaskContractMapper
{
    public static TaskResponse ToResponse(this MissionTask source)
    {
        return new TaskResponse
        {
            Id = source.Id,
            OrganizationId = source.OrganizationId,
            MissionId = source.MissionId,
            Name = source.Name,
            Description = source.Description,
            State = source.State,
            DueDate = source.DueDate
        };
    }
}
