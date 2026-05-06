
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
            EmployeeId = source.EmployeeId,
            Title = source.Title,
            Description = source.Description,
            IsDone = source.IsDone,
            DueDate = source.DueDate,
            SortOrder = source.SortOrder,
            CompletedAt = source.CompletedAt,
            CreatedAt = source.CreatedAt
        };
    }
}
