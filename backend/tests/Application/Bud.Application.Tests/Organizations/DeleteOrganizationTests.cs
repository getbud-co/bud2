namespace Bud.Application.Tests.Organizations;

public sealed class DeleteOrganizationTests
{
    private static IOptions<GlobalAdminSettings> NonProtectedSettings() =>
        Options.Create(new GlobalAdminSettings { OrganizationName = "admin.bud.local" });

    [Fact]
    public async Task ExecuteAsync_WhenFound_ShouldRemoveAndCommit()
    {
        var organization = Organization.Create(Guid.NewGuid(), OrganizationDomainName.Create("empresa.com"));
        var repository = new Mock<IOrganizationRepository>();
        repository.Setup(x => x.GetByIdAsync(organization.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organization);
        repository.Setup(x => x.HasEmployeesAsync(organization.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var unitOfWork = new Mock<IUnitOfWork>();

        var sut = new DeleteOrganization(repository.Object, NonProtectedSettings(), NullLogger<DeleteOrganization>.Instance, unitOfWork.Object);

        var result = await sut.ExecuteAsync(organization.Id);

        result.IsSuccess.Should().BeTrue();
        repository.Verify(x => x.RemoveAsync(organization, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotFound_ShouldReturnNotFound()
    {
        var repository = new Mock<IOrganizationRepository>();
        repository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        var sut = new DeleteOrganization(repository.Object, NonProtectedSettings(), NullLogger<DeleteOrganization>.Instance);

        var result = await sut.ExecuteAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHasEmployees_ShouldReturnConflict()
    {
        var organization = Organization.Create(Guid.NewGuid(), OrganizationDomainName.Create("empresa.com"));
        var repository = new Mock<IOrganizationRepository>();
        repository.Setup(x => x.GetByIdAsync(organization.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organization);
        repository.Setup(x => x.HasEmployeesAsync(organization.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = new DeleteOrganization(repository.Object, NonProtectedSettings(), NullLogger<DeleteOrganization>.Instance);

        var result = await sut.ExecuteAsync(organization.Id);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Conflict);
    }
}
