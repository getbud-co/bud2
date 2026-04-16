using Bud.Application.Ports;
using Bud.Shared.Contracts;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bud.Api.UnitTests.Validators;

public sealed class CreateEmployeeValidatorTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<ITenantProvider> _tenantProvider = new();
    private readonly CreateEmployeeValidator _validator;

    public CreateEmployeeValidatorTests()
    {
        _employeeRepository
            .Setup(x => x.IsEmailUniqueAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _employeeRepository
            .Setup(x => x.IsValidLeaderAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _tenantProvider
            .SetupGet(x => x.TenantId)
            .Returns(Guid.NewGuid());

        _validator = new CreateEmployeeValidator(_employeeRepository.Object, _tenantProvider.Object);
    }

    [Fact]
    public async Task Validate_WithValidData_ShouldPass()
    {
        var request = new CreateEmployeeRequest
        {
            FullName = "John Doe",
            Email = "john.doe@example.com"
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_WithEmptyFullName_ShouldFail(string? fullName)
    {
        var request = new CreateEmployeeRequest
        {
            FullName = fullName!,
            Email = "john.doe@example.com"
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("FullName"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_WithEmptyEmail_ShouldFail(string? email)
    {
        var request = new CreateEmployeeRequest
        {
            FullName = "John Doe",
            Email = email!
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Email"));
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@invalid")]
    [InlineData("invalid.com")]
    public async Task Validate_WithInvalidEmailFormat_ShouldFail(string email)
    {
        var request = new CreateEmployeeRequest
        {
            FullName = "John Doe",
            Email = email
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Email") && e.ErrorMessage.Contains("E-mail deve ser válido"));
    }

    [Fact]
    public async Task Validate_WithDuplicateEmail_ShouldFail()
    {
        _employeeRepository
            .Setup(x => x.IsEmailUniqueAsync("existing@example.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var request = new CreateEmployeeRequest
        {
            FullName = "John Doe",
            Email = "existing@example.com"
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Email") && e.ErrorMessage.Contains("E-mail já está em uso"));
    }

    [Fact]
    public async Task Validate_WithNonExistentLeader_ShouldFail()
    {
        _employeeRepository
            .Setup(x => x.IsValidLeaderAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var request = new CreateEmployeeRequest
        {
            FullName = "John Doe",
            Email = "john.doe@example.com",
            LeaderId = Guid.NewGuid()
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("LeaderId"));
    }
}
