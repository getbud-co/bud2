namespace Bud.Domain.Organizations;

public sealed class Organization : IAggregateRoot
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public OrganizationPlan Plan { get; set; } = OrganizationPlan.Free;
    public OrganizationContractStatus ContractStatus { get; set; } = OrganizationContractStatus.ToApproval;
    public string? IconUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public static Organization Create(
        Guid id,
        string name,
        OrganizationPlan plan = OrganizationPlan.Free,
        OrganizationContractStatus contractStatus = OrganizationContractStatus.ToApproval,
        string? iconUrl = null)
    {
        var organization = new Organization
        {
            Id = id,
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
        Name = OrganizationDomainName.Create(name).Value;
    }
}
