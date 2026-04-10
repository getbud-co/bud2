namespace Bud.Application.Features.Organizations;

public static class OrganizationContractMapper
{
    public static OrganizationResponse ToResponse(this Organization source)
    {
        return new OrganizationResponse
        {
            Id = source.Id,
            Name = source.Name,
            Cnpj = source.Cnpj,
            Plan = source.Plan,
            ContractStatus = source.ContractStatus,
            IconUrl = source.IconUrl,
            CreatedAt = source.CreatedAt,
            DeletedAt = source.DeletedAt
        };
    }
}
