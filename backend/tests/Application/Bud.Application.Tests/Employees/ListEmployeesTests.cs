namespace Bud.Application.Tests.Employees;

public sealed class ListEmployeesTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnPagedResult()
    {
        var employees = new PagedResult<Employee> { Items = [], Total = 0, Page = 1, PageSize = 10 };
        var repository = new Mock<IEmployeeRepository>();
        repository.Setup(x => x.GetAllAsync("search", 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employees);

        var sut = new ListEmployees(repository.Object);

        var result = await sut.ExecuteAsync("search", 1, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
}
