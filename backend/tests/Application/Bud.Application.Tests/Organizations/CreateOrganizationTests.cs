namespace Bud.Application.Tests.Organizations;

public sealed class CreateOrganizationTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidDomain_ShouldCreateAndCommit()
    {
        var repository = new Mock<IOrganizationRepository>();
        repository.Setup(x => x.ExistsByNameAsync(It.IsAny<OrganizationDomainName>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var unitOfWork = new Mock<IUnitOfWork>();

        var sut = new CreateOrganization(repository.Object, NullLogger<CreateOrganization>.Instance, unitOfWork.Object);

        var result = await sut.ExecuteAsync(new CreateOrganizationCommand("nova-empresa.com"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Value.Should().Be("nova-empresa.com");
        repository.Verify(x => x.AddAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateName_ShouldReturnConflict()
    {
        var repository = new Mock<IOrganizationRepository>();
        repository.Setup(x => x.ExistsByNameAsync(It.IsAny<OrganizationDomainName>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = new CreateOrganization(repository.Object, NullLogger<CreateOrganization>.Instance);

        var result = await sut.ExecuteAsync(new CreateOrganizationCommand("existente.com"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidDomain_ShouldReturnValidation()
    {
        var repository = new Mock<IOrganizationRepository>();

        var sut = new CreateOrganization(repository.Object, NullLogger<CreateOrganization>.Instance);

        var result = await sut.ExecuteAsync(new CreateOrganizationCommand("not a domain"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
    }
}
