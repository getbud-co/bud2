namespace Bud.Application.Tests.Organizations;

public sealed class GetOrganizationByIdTests
{
    [Fact]
    public async Task ExecuteAsync_WhenFound_ShouldReturnOrganization()
    {
        var organization = Organization.Create(Guid.NewGuid(), OrganizationDomainName.Create("empresa.com"));
        var repository = new Mock<IOrganizationRepository>();
        repository.Setup(x => x.GetByIdAsync(organization.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organization);

        var sut = new GetOrganizationById(repository.Object);

        var result = await sut.ExecuteAsync(organization.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(organization);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotFound_ShouldReturnNotFound()
    {
        var repository = new Mock<IOrganizationRepository>();
        repository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        var sut = new GetOrganizationById(repository.Object);

        var result = await sut.ExecuteAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }
}
