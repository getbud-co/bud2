using Bud.Shared.Contracts;
using FluentAssertions;
using Xunit;

namespace Bud.Api.UnitTests.Validators;

public sealed class PatchMissionValidatorTests
{
    private readonly PatchMissionValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidRequest_Passes()
    {
        var request = new PatchMissionRequest
        {
            Name = "Missão Atualizada",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30),
            Status = MissionStatus.Active
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithNoFieldsSet_Passes()
    {
        // PatchMissionValidator uses .When guards on all fields,
        // so a default PatchMissionRequest with no fields set passes validation.
        var request = new PatchMissionRequest();

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    #region Name Validation

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_WithEmptyName_Fails(string? name)
    {
        var request = new PatchMissionRequest
        {
            Name = name!
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Name"));
    }

    [Fact]
    public async Task Validate_WithNameExceeding200Characters_Fails()
    {
        var request = new PatchMissionRequest
        {
            Name = new string('A', 201)
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName.Contains("Name") &&
            e.ErrorMessage.Contains("200"));
    }

    #endregion

    #region Status Validation

    [Fact]
    public async Task Validate_WithInvalidStatus_Fails()
    {
        var request = new PatchMissionRequest
        {
            Status = (MissionStatus)999
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName.Contains("Status") &&
            e.ErrorMessage.Contains("Status inválido"));
    }

    #endregion

    #region EndDate Validation

    [Fact]
    public async Task Validate_WithEmptyEndDate_Fails()
    {
        var request = new PatchMissionRequest
        {
            EndDate = default(DateTime)
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("EndDate"));
    }

    #endregion

    #region Date Range Validation

    [Fact]
    public async Task Validate_WithEndDateBeforeStartDate_Fails()
    {
        var request = new PatchMissionRequest
        {
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage.Contains("Data de término deve ser igual ou posterior à data de início"));
    }

    [Fact]
    public async Task Validate_WithEndDateEqualToStartDate_Passes()
    {
        var date = DateTime.UtcNow;
        var request = new PatchMissionRequest
        {
            StartDate = date,
            EndDate = date
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithOnlyStartDate_Passes()
    {
        var request = new PatchMissionRequest
        {
            StartDate = DateTime.UtcNow
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithOnlyEndDate_Passes()
    {
        // All fields use .When guards, so setting only EndDate is valid.
        var request = new PatchMissionRequest
        {
            EndDate = DateTime.UtcNow.AddDays(30)
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}
