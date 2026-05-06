using Bud.Application.Features.Employees;
using Bud.Application.Features.Indicators;
using Bud.Application.Features.Tags;
using Bud.Application.Features.Tasks;

namespace Bud.Application.Features.Missions;

public static class MissionContractMapper
{
    public static MissionResponse ToResponse(this Mission source)
    {
        return new MissionResponse
        {
            Id = source.Id,
            Title = source.Title,
            Description = source.Description,
            Dimension = source.Dimension,
            DueDate = source.DueDate,
            CompletedAt = source.CompletedAt,
            Status = source.Status,
            Visibility = source.Visibility,
            KanbanStatus = source.KanbanStatus,
            OrganizationId = source.OrganizationId,
            CycleId = source.CycleId,
            ParentId = source.ParentId,
            Path = source.Path,
            SortOrder = source.SortOrder,
            EmployeeId = source.EmployeeId,
            Employee = source.Employee?.ToEmployeeMembershipResponse(),
            CreatedAt = source.CreatedAt,
            Children = source.Children.Select(c => c.ToResponse()).ToList(),
            Indicators = source.Indicators.Select(i => i.ToResponse()).ToList(),
            Tasks = source.Tasks.Select(t => t.ToResponse()).ToList(),
            Tags = source.Tags.Select(mt => mt.Tag.ToResponse()).ToList()
        };
    }
}
