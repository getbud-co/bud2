namespace Bud.Application.Tests.Organizations;

public sealed class ListOrganizationsTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnPagedResult()
    {
        var organizations = new PagedResult<Organization> { Items = [], Total = 0, Page = 1, PageSize = 10 };
        var repository = new Mock<IOrganizationRepository>();
        repository.Setup(x => x.GetAllAsync("search", 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organizations);

        var sut = new ListOrganizations(repository.Object);

        var result = await sut.ExecuteAsync("search", 1, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
}
