namespace Bud.Domain.Organizations;

public sealed class Organization : IAggregateRoot
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public OrganizationPlan Plan { get; set; } = OrganizationPlan.Free;
    public OrganizationContractStatus ContractStatus { get; set; } = OrganizationContractStatus.ToApproval;
    public string? IconUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public static Organization Create(
        Guid id,
        string name,
        string cnpj,
        OrganizationPlan plan = OrganizationPlan.Free,
        OrganizationContractStatus contractStatus = OrganizationContractStatus.ToApproval,
        string? iconUrl = null)
    {
        var organization = new Organization
        {
            Id = id,
            Cnpj = cnpj,
            Plan = plan,
            ContractStatus = contractStatus,
            IconUrl = iconUrl,
            CreatedAt = DateTime.UtcNow
        };

        organization.Rename(name);
        return organization;
    }

    public void Rename(string name)
    {
        if (!EntityName.TryCreate(name, out EntityName entityName))
        {
            throw new DomainInvariantException("O nome da organização é obrigatório e deve ter até 200 caracteres.");
        }

        Name = entityName.Value;
    }
}
