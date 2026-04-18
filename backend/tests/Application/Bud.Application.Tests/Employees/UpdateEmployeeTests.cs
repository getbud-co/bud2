namespace Bud.Application.Tests.Employees;

public sealed class UpdateEmployeeTests
{
    private static readonly EmployeeName ValidName = EmployeeName.Create("João Silva");
    private static readonly EmailAddress ValidEmail = EmailAddress.Create("joao@bud.co");

    private static Employee CreateEmployee() =>
        Employee.Create(Guid.NewGuid(), Guid.NewGuid(), ValidName, ValidEmail, EmployeeRole.IndividualContributor);

    [Fact]
    public async Task ExecuteAsync_WhenNotFound_ShouldReturnNotFound()
    {
        var repository = new Mock<IEmployeeRepository>();
        repository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var sut = new UpdateEmployee(repository.Object, NullLogger<UpdateEmployee>.Instance);

        var result = await sut.ExecuteAsync(Guid.NewGuid(), new UpdateEmployeeCommand(default, default, default));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidName_ShouldUpdateAndCommit()
    {
        var employee = CreateEmployee();
        var repository = new Mock<IEmployeeRepository>();
        repository.Setup(x => x.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        var unitOfWork = new Mock<IUnitOfWork>();

        var sut = new UpdateEmployee(repository.Object, NullLogger<UpdateEmployee>.Instance, unitOfWork.Object);

        var result = await sut.ExecuteAsync(employee.Id, new UpdateEmployeeCommand("Maria Santos", default, default));

        result.IsSuccess.Should().BeTrue();
        result.Value!.FullName.Value.Should().Be("Maria Santos");
        unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateEmail_ShouldReturnValidation()
    {
        var employee = CreateEmployee();
        var repository = new Mock<IEmployeeRepository>();
        repository.Setup(x => x.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        repository.Setup(x => x.IsEmailUniqueAsync(It.IsAny<EmailAddress>(), employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var sut = new UpdateEmployee(repository.Object, NullLogger<UpdateEmployee>.Instance);

        var result = await sut.ExecuteAsync(employee.Id, new UpdateEmployeeCommand(default, "outro@bud.co", default));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
    }
}
