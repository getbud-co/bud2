namespace Bud.Domain.Organizations;

public sealed class Organization : IAggregateRoot
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public static Organization Create(Guid id, string name)
    {
        var organization = new Organization
        {
            Id = id
        };

        organization.Rename(name);
        return organization;
    }

    public void Rename(string name)
    {
        Name = OrganizationDomainName.Create(name).Value;
    }
}
