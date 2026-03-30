using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Bud.Application.UnitTests.Application.Checkins;

public sealed class CheckinWriteUseCasesTests
{
    private static readonly ClaimsPrincipal User = new(new ClaimsIdentity());

    [Fact]
    public async Task CreateAsync_WhenEmployeeNotIdentified_ReturnsForbidden()
    {
        var orgId = Guid.NewGuid();
        var mission = new Mission
        {
            Id = Guid.NewGuid(),
            Name = "Missão",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = MissionStatus.Active,
            OrganizationId = orgId
        };
        var metric = new Indicator
        {
            Id = Guid.NewGuid(),
            Name = "Métrica",
            Type = IndicatorType.Qualitative,
            MissionId = mission.Id,
            Mission = mission,
            OrganizationId = orgId
        };

        var checkinRepository = new Mock<IIndicatorRepository>(MockBehavior.Strict);
        checkinRepository
            .Setup(r => r.GetIndicatorWithMissionAsync(metric.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(metric);

        var employeeRepository = new Mock<IEmployeeRepository>(MockBehavior.Strict);

        var tenantProvider = new Mock<ITenantProvider>();
        tenantProvider.SetupGet(t => t.IsGlobalAdmin).Returns(false);
        tenantProvider.SetupGet(t => t.EmployeeId).Returns((Guid?)null);

        var useCase = new CreateCheckin(
            checkinRepository.Object,
            employeeRepository.Object,
            tenantProvider.Object,
            NullLogger<CreateCheckin>.Instance);

        var result = await useCase.ExecuteAsync(metric.Id, new CreateCheckinCommand(null, "ok", DateTime.UtcNow, null, 3));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        result.Error.Should().Be("Funcionário não identificado.");
        employeeRepository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAsync_WhenEmployeeDoesNotExist_ReturnsNotFound()
    {
        var orgId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var mission = new Mission
        {
            Id = Guid.NewGuid(),
            Name = "Meta",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = MissionStatus.Active,
            OrganizationId = orgId
        };
        var indicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Name = "Indicador",
            Type = IndicatorType.Qualitative,
            MissionId = mission.Id,
            Mission = mission,
            OrganizationId = orgId
        };

        var indicatorRepository = new Mock<IIndicatorRepository>(MockBehavior.Strict);
        indicatorRepository
            .Setup(r => r.GetIndicatorWithMissionAsync(indicator.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(indicator);

        var employeeRepository = new Mock<IEmployeeRepository>(MockBehavior.Strict);
        employeeRepository
            .Setup(r => r.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var tenantProvider = new Mock<ITenantProvider>(MockBehavior.Strict);
        tenantProvider.SetupGet(t => t.EmployeeId).Returns(employeeId);

        var useCase = new CreateCheckin(
            indicatorRepository.Object,
            employeeRepository.Object,
            tenantProvider.Object,
            NullLogger<CreateCheckin>.Instance);

        var result = await useCase.ExecuteAsync(indicator.Id, new CreateCheckinCommand(null, "ok", DateTime.UtcNow, null, 3));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Error.Should().Be("Funcionário não encontrado.");
    }

    [Fact]
    public async Task CreateAsync_WhenSuccess_RegistersCheckinCreatedDomainEvent()
    {
        var orgId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var mission = new Mission
        {
            Id = Guid.NewGuid(),
            Name = "Missão",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = MissionStatus.Active,
            OrganizationId = orgId
        };
        var metric = new Indicator
        {
            Id = Guid.NewGuid(),
            Name = "Métrica",
            Type = IndicatorType.Quantitative,
            MissionId = mission.Id,
            Mission = mission,
            OrganizationId = orgId,
            QuantitativeType = QuantitativeIndicatorType.KeepAbove,
            MinValue = 0m,
            Unit = IndicatorUnit.Integer
        };

        var checkinRepository = new Mock<IIndicatorRepository>();
        checkinRepository
            .Setup(r => r.GetIndicatorWithMissionAsync(metric.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(metric);
        checkinRepository
            .Setup(r => r.AddCheckinAsync(It.IsAny<Checkin>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        checkinRepository
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var employeeRepository = new Mock<IEmployeeRepository>();
        employeeRepository
            .Setup(r => r.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Employee { Id = employeeId, FullName = "Test", Email = "test@test.com", OrganizationId = orgId });

        var tenantProvider = new Mock<ITenantProvider>();
        tenantProvider.SetupGet(t => t.IsGlobalAdmin).Returns(false);
        tenantProvider.SetupGet(t => t.EmployeeId).Returns(employeeId);

        var useCase = new CreateCheckin(
            checkinRepository.Object,
            employeeRepository.Object,
            tenantProvider.Object,
            NullLogger<CreateCheckin>.Instance);

        var result = await useCase.ExecuteAsync(metric.Id, new CreateCheckinCommand(10m, null, DateTime.UtcNow, null, 3));

        result.IsSuccess.Should().BeTrue();
        checkinRepository.Verify(r => r.AddCheckinAsync(It.IsAny<Checkin>(), It.IsAny<CancellationToken>()), Times.Once);
        checkinRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        var createdEvent = metric.DomainEvents.Should().ContainSingle().Subject;
        var created = createdEvent.Should().BeOfType<CheckinCreatedDomainEvent>().Subject;
        created.CheckinId.Should().Be(result.Value!.Id);
        created.IndicatorId.Should().Be(metric.Id);
        created.OrganizationId.Should().Be(orgId);
        created.EmployeeId.Should().Be(employeeId);
    }

    [Fact]
    public async Task CreateAsync_WhenMissionNotActive_ReturnsValidation()
    {
        var orgId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var mission = new Mission
        {
            Id = Guid.NewGuid(),
            Name = "Missão",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = MissionStatus.Planned,
            OrganizationId = orgId
        };
        var metric = new Indicator
        {
            Id = Guid.NewGuid(),
            Name = "Métrica",
            Type = IndicatorType.Quantitative,
            MissionId = mission.Id,
            Mission = mission,
            OrganizationId = orgId
        };

        var checkinRepository = new Mock<IIndicatorRepository>(MockBehavior.Strict);
        checkinRepository
            .Setup(r => r.GetIndicatorWithMissionAsync(metric.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(metric);

        var employeeRepository = new Mock<IEmployeeRepository>();
        employeeRepository
            .Setup(r => r.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Employee { Id = employeeId, FullName = "Test", Email = "test@test.com", OrganizationId = orgId });

        var tenantProvider = new Mock<ITenantProvider>();
        tenantProvider.SetupGet(t => t.IsGlobalAdmin).Returns(false);
        tenantProvider.SetupGet(t => t.EmployeeId).Returns(employeeId);

        var useCase = new CreateCheckin(
            checkinRepository.Object,
            employeeRepository.Object,
            tenantProvider.Object,
            NullLogger<CreateCheckin>.Instance);

        var result = await useCase.ExecuteAsync(metric.Id, new CreateCheckinCommand(10m, null, DateTime.UtcNow, null, 3));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.Error.Should().Be("Não é possível fazer check-in em indicadores de metas que não estão ativas.");
    }

    [Fact]
    public async Task UpdateAsync_WhenNotAuthorAndNotGlobalAdmin_ReturnsForbidden()
    {
        var checkin = new Checkin
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            CheckinDate = DateTime.UtcNow,
            ConfidenceLevel = 3
        };

        var checkinRepository = new Mock<IIndicatorRepository>(MockBehavior.Strict);
        checkinRepository
            .Setup(r => r.GetCheckinByIdAsync(checkin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkin);

        var authorizationGateway = new Mock<IApplicationAuthorizationGateway>();
        authorizationGateway
            .Setup(g => g.CanWriteAsync(User, It.IsAny<IndicatorResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var useCase = new PatchCheckin(
            checkinRepository.Object,
            authorizationGateway.Object,
            NullLogger<PatchCheckin>.Instance);

        var result = await useCase.ExecuteAsync(User, checkin.IndicatorId, checkin.Id, new PatchCheckinCommand(10m, null, DateTime.UtcNow, null, 2));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        result.Error.Should().Be("Você não tem permissão para atualizar este check-in.");
    }

    [Fact]
    public async Task UpdateAsync_WhenGlobalAdmin_UpdatesViaRepository()
    {
        var orgId = Guid.NewGuid();
        var indicatorId = Guid.NewGuid();
        var checkin = new Checkin
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            OrganizationId = orgId,
            IndicatorId = indicatorId,
            CheckinDate = DateTime.UtcNow,
            Value = 10m,
            ConfidenceLevel = 3
        };
        var metric = new Indicator
        {
            Id = indicatorId,
            Name = "Métrica",
            Type = IndicatorType.Quantitative,
            MissionId = Guid.NewGuid(),
            OrganizationId = orgId,
            QuantitativeType = QuantitativeIndicatorType.KeepAbove,
            MinValue = 0m,
            Unit = IndicatorUnit.Integer
        };

        var checkinRepository = new Mock<IIndicatorRepository>();
        checkinRepository
            .Setup(r => r.GetCheckinByIdAsync(checkin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkin);
        checkinRepository
            .Setup(r => r.GetByIdAsync(indicatorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(metric);
        checkinRepository
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var authorizationGateway = new Mock<IApplicationAuthorizationGateway>();
        authorizationGateway
            .Setup(g => g.CanWriteAsync(User, It.IsAny<IndicatorResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = new PatchCheckin(
            checkinRepository.Object,
            authorizationGateway.Object,
            NullLogger<PatchCheckin>.Instance);

        var result = await useCase.ExecuteAsync(User, checkin.IndicatorId, checkin.Id, new PatchCheckinCommand(25m, null, DateTime.UtcNow, null, 4));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Value.Should().Be(25m);
        checkinRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenGlobalAdmin_RemovesViaRepository()
    {
        var checkin = new Checkin
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            CheckinDate = DateTime.UtcNow,
            ConfidenceLevel = 3
        };

        var checkinRepository = new Mock<IIndicatorRepository>();
        checkinRepository
            .Setup(r => r.GetCheckinByIdAsync(checkin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkin);
        checkinRepository
            .Setup(r => r.RemoveCheckinAsync(checkin, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        checkinRepository
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var authorizationGateway = new Mock<IApplicationAuthorizationGateway>();
        authorizationGateway
            .Setup(g => g.CanWriteAsync(User, It.IsAny<IndicatorResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = new DeleteCheckin(
            checkinRepository.Object,
            authorizationGateway.Object,
            NullLogger<DeleteCheckin>.Instance);

        var result = await useCase.ExecuteAsync(User, checkin.IndicatorId, checkin.Id);

        result.IsSuccess.Should().BeTrue();
        checkinRepository.Verify(r => r.RemoveCheckinAsync(checkin, It.IsAny<CancellationToken>()), Times.Once);
        checkinRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
