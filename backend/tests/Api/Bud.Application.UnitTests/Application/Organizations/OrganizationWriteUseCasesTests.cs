using Bud.Application.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Bud.Application.UnitTests.Application.Organizations;

public sealed class OrganizationUseCasesTests
{
    private readonly Mock<IOrganizationRepository> _orgRepo = new();

    private static IOptions<GlobalAdminSettings> CreateSettings(string protectedOrgName = "getbud.co")
        => Options.Create(new GlobalAdminSettings
        {
            Email = "admin@getbud.co",
            OrganizationName = protectedOrgName
        });

    private CreateOrganization CreateRegisterOrganization()
        => new(_orgRepo.Object, NullLogger<CreateOrganization>.Instance);

    private PatchOrganization CreateRenameOrganization(string protectedOrgName = "getbud.co")
        => new(_orgRepo.Object, CreateSettings(protectedOrgName), NullLogger<PatchOrganization>.Instance);

    private DeleteOrganization CreateDeleteOrganization(string protectedOrgName = "getbud.co")
        => new(_orgRepo.Object, CreateSettings(protectedOrgName), NullLogger<DeleteOrganization>.Instance);

    [Fact]
    public async Task CreateAsync_WithValidRequest_ReturnsSuccess()
    {
        var useCase = CreateRegisterOrganization();

        var result = await useCase.ExecuteAsync(new CreateOrganizationCommand("test-org.com", "", OrganizationPlan.Free, OrganizationContractStatus.ToApproval, ""));

        result.IsSuccess.Should().BeTrue();
        _orgRepo.Verify(r => r.AddAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()), Times.Once);
        _orgRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ReturnsValidationError()
    {
        var useCase = CreateRegisterOrganization();

        var result = await useCase.ExecuteAsync(new CreateOrganizationCommand("", "", OrganizationPlan.Free, OrganizationContractStatus.ToApproval, ""));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.Error.Should().Be("O nome da organização é obrigatório e deve ter até 200 caracteres.");
        _orgRepo.Verify(r => r.AddAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()), Times.Never);
        _orgRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentOrganization_ReturnsNotFound()
    {
        _orgRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        var useCase = CreateRenameOrganization();

        var result = await useCase.ExecuteAsync(Guid.NewGuid(), new PatchOrganizationCommand("New Name", default, default, default, ""));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Error.Should().Be("Organização não encontrada.");
    }

    [Fact]
    public async Task UpdateAsync_ProtectedOrganization_ReturnsValidationError()
    {
        var orgId = Guid.NewGuid();
        var org = new Organization { Id = orgId, Name = "getbud.co" };
        _orgRepo.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>())).ReturnsAsync(org);

        var useCase = CreateRenameOrganization("getbud.co");

        var result = await useCase.ExecuteAsync(orgId, new PatchOrganizationCommand("New Name", default, default, default, ""));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.Error.Should().Be("Esta organização está protegida e não pode ser alterada.");
    }

    [Fact]
    public async Task UpdateAsync_WithValidOrganization_RenamesSuccessfully()
    {
        var orgId = Guid.NewGuid();
        var org = new Organization { Id = orgId, Name = "Test Org" };
        _orgRepo.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>())).ReturnsAsync(org);

        var useCase = CreateRenameOrganization();

        var result = await useCase.ExecuteAsync(orgId, new PatchOrganizationCommand("updated.com", default, default, default, ""));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("updated.com");
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyName_ReturnsValidationError()
    {
        var orgId = Guid.NewGuid();
        var org = new Organization { Id = orgId, Name = "test-org.com" };
        _orgRepo.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>())).ReturnsAsync(org);

        var useCase = CreateRenameOrganization();

        var result = await useCase.ExecuteAsync(orgId, new PatchOrganizationCommand("", default, default, default, ""));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.Error.Should().Be("O nome da organização é obrigatório e deve ter até 200 caracteres.");
        _orgRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentOrganization_ReturnsNotFound()
    {
        _orgRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        var useCase = CreateDeleteOrganization();

        var result = await useCase.ExecuteAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Error.Should().Be("Organização não encontrada.");
    }

    [Fact]
    public async Task DeleteAsync_ProtectedOrganization_ReturnsValidationError()
    {
        var orgId = Guid.NewGuid();
        var org = new Organization { Id = orgId, Name = "getbud.co" };
        _orgRepo.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>())).ReturnsAsync(org);

        var useCase = CreateDeleteOrganization("getbud.co");

        var result = await useCase.ExecuteAsync(orgId);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.Error.Should().Be("Esta organização está protegida e não pode ser excluída.");
    }

    [Fact]
    public async Task DeleteAsync_WithEmployees_ReturnsConflict()
    {
        var orgId = Guid.NewGuid();
        var org = new Organization { Id = orgId, Name = "test-org.com" };
        _orgRepo.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>())).ReturnsAsync(org);
        _orgRepo.Setup(r => r.HasEmployeesAsync(orgId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var useCase = CreateDeleteOrganization();

        var result = await useCase.ExecuteAsync(orgId);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Conflict);
        result.Error.Should().Be("Não é possível excluir a organização porque ela possui colaboradores associados. Remova os colaboradores primeiro.");
    }

    [Fact]
    public async Task DeleteAsync_WithValidOrganization_Succeeds()
    {
        var orgId = Guid.NewGuid();
        var org = new Organization { Id = orgId, Name = "test-org.com" };
        _orgRepo.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>())).ReturnsAsync(org);
        _orgRepo.Setup(r => r.HasEmployeesAsync(orgId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var useCase = CreateDeleteOrganization();

        var result = await useCase.ExecuteAsync(orgId);

        result.IsSuccess.Should().BeTrue();
        _orgRepo.Verify(r => r.RemoveAsync(org, It.IsAny<CancellationToken>()), Times.Once);
        _orgRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
