namespace Bud.Application.Tests.Organizations;

public sealed class OrganizationProtectionUseCaseTests
{
    [Fact]
    public async Task UpdateOrganization_WhenProtected_ShouldReturnConflict()
    {
        var protectedOrganization = Organization.Create(Guid.NewGuid(), OrganizationDomainName.Create("admin.bud.local"));
        var repository = new Mock<IOrganizationRepository>();
        repository.Setup(x => x.GetByIdAsync(protectedOrganization.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(protectedOrganization);

        var settings = Options.Create(new GlobalAdminSettings
        {
            OrganizationName = "admin.bud.local"
        });

        var sut = new UpdateOrganization(repository.Object, settings, NullLogger<UpdateOrganization>.Instance);

        var result = await sut.ExecuteAsync(protectedOrganization.Id, new UpdateOrganizationCommand("novo-dominio.com"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task DeleteOrganization_WhenProtected_ShouldReturnConflict()
    {
        var protectedOrganization = Organization.Create(Guid.NewGuid(), OrganizationDomainName.Create("admin.bud.local"));
        var repository = new Mock<IOrganizationRepository>();
        repository.Setup(x => x.GetByIdAsync(protectedOrganization.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(protectedOrganization);

        var settings = Options.Create(new GlobalAdminSettings
        {
            OrganizationName = "admin.bud.local"
        });

        var sut = new DeleteOrganization(repository.Object, settings, NullLogger<DeleteOrganization>.Instance);

        var result = await sut.ExecuteAsync(protectedOrganization.Id);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Conflict);
    }
}
