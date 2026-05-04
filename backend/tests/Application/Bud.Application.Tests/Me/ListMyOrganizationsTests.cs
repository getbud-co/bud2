namespace Bud.Application.Tests.Me;

public sealed class ListMyOrganizationsTests
{
    [Fact]
    public async Task ExecuteAsync_WhenSuccess_ShouldReturnMappedOrganizations()
    {
        var snapshots = new List<OrganizationSnapshot>
        {
            new() { Id = Guid.NewGuid(), Name = "empresa.com" },
            new() { Id = Guid.NewGuid(), Name = "outra.com.br" }
        };
        var readStore = new Mock<IMyOrganizationsReadStore>();
        readStore.Setup(x => x.GetMyOrganizationsAsync("user@bud.co", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<OrganizationSnapshot>>.Success(snapshots));

        var sut = new ListMyOrganizations(readStore.Object);

        var result = await sut.ExecuteAsync("user@bud.co");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoreFails_ShouldReturnFailure()
    {
        var readStore = new Mock<IMyOrganizationsReadStore>();
        readStore.Setup(x => x.GetMyOrganizationsAsync("user@bud.co", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<OrganizationSnapshot>>.Failure("Falha ao carregar organizações.", ErrorType.Validation));

        var sut = new ListMyOrganizations(readStore.Object);

        var result = await sut.ExecuteAsync("user@bud.co");

        result.IsSuccess.Should().BeFalse();
    }
}
