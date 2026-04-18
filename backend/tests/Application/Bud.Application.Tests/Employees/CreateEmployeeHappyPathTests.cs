namespace Bud.Application.Tests.Employees;

public sealed class CreateEmployeeHappyPathTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidData_ShouldCreateAndCommit()
    {
        var tenantProvider = new TestTenantProvider { TenantId = Guid.NewGuid() };
        var repository = new Mock<IEmployeeRepository>();
        repository.Setup(x => x.IsEmailUniqueAsync(It.IsAny<EmailAddress>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var unitOfWork = new Mock<IUnitOfWork>();

        var sut = new CreateEmployee(repository.Object, tenantProvider, NullLogger<CreateEmployee>.Instance, unitOfWork.Object);

        var result = await sut.ExecuteAsync(new CreateEmployeeCommand("João Silva", "joao@bud.co", EmployeeRole.Leader));

        result.IsSuccess.Should().BeTrue();
        result.Value!.FullName.Value.Should().Be("João Silva");
        result.Value.Email.Value.Should().Be("joao@bud.co");
        result.Value.Role.Should().Be(EmployeeRole.Leader);
        repository.Verify(x => x.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutTenantContext_ShouldReturnValidation()
    {
        var tenantProvider = new TestTenantProvider { TenantId = null };
        var repository = new Mock<IEmployeeRepository>();

        var sut = new CreateEmployee(repository.Object, tenantProvider, NullLogger<CreateEmployee>.Instance);

        var result = await sut.ExecuteAsync(new CreateEmployeeCommand("João Silva", "joao@bud.co", EmployeeRole.Leader));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidEmail_ShouldReturnValidation()
    {
        var tenantProvider = new TestTenantProvider { TenantId = Guid.NewGuid() };
        var repository = new Mock<IEmployeeRepository>();

        var sut = new CreateEmployee(repository.Object, tenantProvider, NullLogger<CreateEmployee>.Instance);

        var result = await sut.ExecuteAsync(new CreateEmployeeCommand("João Silva", "invalid", EmployeeRole.Leader));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
    }
}
