namespace Bud.Domain.Tests.Organizations;

public sealed class OrganizationTests
{
    [Fact]
    public void Create_WithValidData_ShouldInitializeCorrectly()
    {
        var id = Guid.NewGuid();
        var name = OrganizationDomainName.Create("empresa.com.br");

        var organization = Organization.Create(id, name);

        organization.Id.Should().Be(id);
        organization.Name.Should().Be(name);
    }

    [Fact]
    public void Rename_ShouldUpdateName()
    {
        var organization = Organization.Create(Guid.NewGuid(), OrganizationDomainName.Create("old.com"));
        var newName = OrganizationDomainName.Create("new-company.com");

        organization.Rename(newName);

        organization.Name.Should().Be(newName);
    }
}
