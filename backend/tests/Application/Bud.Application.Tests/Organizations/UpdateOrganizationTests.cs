namespace Bud.Application.Tests.Organizations;

public sealed class UpdateOrganizationTests
{
    private static IOptions<GlobalAdminSettings> NonProtectedSettings() =>
        Options.Create(new GlobalAdminSettings { OrganizationName = "admin.bud.local" });

    [Fact]
    public async Task ExecuteAsync_WithValidName_ShouldUpdateAndCommit()
    {
        var organization = Organization.Create(Guid.NewGuid(), OrganizationDomainName.Create("empresa.com"));
        var repository = new Mock<IOrganizationRepository>();
        repository.Setup(x => x.GetByIdAsync(organization.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organization);
        repository.Setup(x => x.ExistsByNameAsync(It.IsAny<OrganizationDomainName>(), organization.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var unitOfWork = new Mock<IUnitOfWork>();

        var sut = new UpdateOrganization(repository.Object, NonProtectedSettings(), NullLogger<UpdateOrganization>.Instance, unitOfWork.Object);

        var result = await sut.ExecuteAsync(organization.Id, new UpdateOrganizationCommand("novo-dominio.com"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Value.Should().Be("novo-dominio.com");
        unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotFound_ShouldReturnNotFound()
    {
        var repository = new Mock<IOrganizationRepository>();
        repository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        var sut = new UpdateOrganization(repository.Object, NonProtectedSettings(), NullLogger<UpdateOrganization>.Instance);

        var result = await sut.ExecuteAsync(Guid.NewGuid(), new UpdateOrganizationCommand("novo.com"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateName_ShouldReturnConflict()
    {
        var organization = Organization.Create(Guid.NewGuid(), OrganizationDomainName.Create("empresa.com"));
        var repository = new Mock<IOrganizationRepository>();
        repository.Setup(x => x.GetByIdAsync(organization.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organization);
        repository.Setup(x => x.ExistsByNameAsync(It.IsAny<OrganizationDomainName>(), organization.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = new UpdateOrganization(repository.Object, NonProtectedSettings(), NullLogger<UpdateOrganization>.Instance);

        var result = await sut.ExecuteAsync(organization.Id, new UpdateOrganizationCommand("existente.com"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Conflict);
    }
}
