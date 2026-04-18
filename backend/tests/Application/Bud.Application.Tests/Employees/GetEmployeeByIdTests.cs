namespace Bud.Application.Tests.Employees;

public sealed class GetEmployeeByIdTests
{
    [Fact]
    public async Task ExecuteAsync_WhenFound_ShouldReturnEmployee()
    {
        var employee = Employee.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            EmployeeName.Create("João Silva"), EmailAddress.Create("joao@bud.co"),
            EmployeeRole.Leader);

        var repository = new Mock<IEmployeeRepository>();
        repository.Setup(x => x.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var sut = new GetEmployeeById(repository.Object);

        var result = await sut.ExecuteAsync(employee.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(employee);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotFound_ShouldReturnNotFound()
    {
        var repository = new Mock<IEmployeeRepository>();
        repository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var sut = new GetEmployeeById(repository.Object);

        var result = await sut.ExecuteAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }
}
