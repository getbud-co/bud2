namespace Bud.Application.Features.Cycles;

public static class CycleContractMapper
{
    public static CycleResponse ToResponse(this Cycle source)
    {
        return new CycleResponse
        {
            Id = source.Id,
            OrganizationId = source.OrganizationId,
            Name = source.Name,
            Cadence = source.Cadence,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            Status = source.Status
        };
    }
}
