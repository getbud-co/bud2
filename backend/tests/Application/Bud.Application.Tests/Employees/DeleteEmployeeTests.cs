namespace Bud.Application.Tests.Employees;

public sealed class DeleteEmployeeTests
{
    [Fact]
    public async Task ExecuteAsync_WhenFound_ShouldRemoveAndCommit()
    {
        var employee = Employee.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            EmployeeName.Create("João Silva"), EmailAddress.Create("joao@bud.co"),
            EmployeeRole.IndividualContributor);

        var repository = new Mock<IEmployeeRepository>();
        repository.Setup(x => x.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        var unitOfWork = new Mock<IUnitOfWork>();

        var sut = new DeleteEmployee(repository.Object, NullLogger<DeleteEmployee>.Instance, unitOfWork.Object);

        var result = await sut.ExecuteAsync(employee.Id);

        result.IsSuccess.Should().BeTrue();
        repository.Verify(x => x.RemoveAsync(employee, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotFound_ShouldReturnNotFound()
    {
        var repository = new Mock<IEmployeeRepository>();
        repository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var sut = new DeleteEmployee(repository.Object, NullLogger<DeleteEmployee>.Instance);

        var result = await sut.ExecuteAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }
}
