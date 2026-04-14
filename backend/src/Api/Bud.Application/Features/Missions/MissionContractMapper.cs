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
            Name = source.Name,
            Description = source.Description,
            Dimension = source.Dimension,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            Status = source.Status,
            OrganizationId = source.OrganizationId,
            ParentId = source.ParentId,
            EmployeeId = source.EmployeeId,
            Employee = source.Employee?.ToEmployeeResponse(),
            Children = source.Children.Select(c => c.ToResponse()).ToList(),
            Indicators = source.Indicators.Select(i => i.ToResponse()).ToList(),
            Tasks = source.Tasks.Select(t => t.ToResponse()).ToList(),
            Tags = source.Tags.Select(mt => mt.Tag.ToResponse()).ToList()
        };
    }
}
