namespace Bud.Domain.Organizations;

public sealed class Organization : IAggregateRoot
{
    public Guid Id { get; set; }
    public OrganizationDomainName Name { get; private set; }

    public static Organization Create(Guid id, OrganizationDomainName name)
    {
        var organization = new Organization
        {
            Id = id
        };

        organization.Rename(name);
        return organization;
    }

    public void Rename(OrganizationDomainName name)
    {
        Name = name;
    }
}
