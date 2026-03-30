using FluentAssertions;
using Xunit;

namespace Bud.Domain.UnitTests.Domain.Models;

public sealed class AggregateInvariantsTests
{
    [Fact]
    public void Organization_Rename_WithEmptyName_ShouldThrow()
    {
        var organization = Organization.Create(Guid.NewGuid(), "Org");

        var act = () => organization.Rename("  ");

        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void Organization_Rename_WithNameLongerThan200_ShouldThrow()
    {
        var organization = Organization.Create(Guid.NewGuid(), "Org");
        var longName = new string('A', 201);

        var act = () => organization.Rename(longName);

        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void Team_Reparent_ToSelf_ShouldThrow()
    {
        var id = Guid.NewGuid();
        var team = Team.Create(id, Guid.NewGuid(), "Team", Guid.NewGuid(), Guid.NewGuid());

        var act = () => team.Reparent(id, id);

        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void Employee_UpdateProfile_WithSelfLeader_ShouldThrow()
    {
        var employee = Employee.Create(Guid.NewGuid(), Guid.NewGuid(), "Ana", "ana@getbud.co", EmployeeRole.Leader);

        var act = () => employee.UpdateProfile("Ana", "ana@getbud.co", EmployeeRole.Leader, employee.Id, employee.Id);

        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void Mission_UpdateDetails_WithInvalidDateRange_ShouldThrow()
    {
        var mission = Mission.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Meta",
            null,
            null,
            new DateTime(2026, 2, 12, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 2, 13, 0, 0, 0, DateTimeKind.Utc),
            MissionStatus.Active);

        var act = () => mission.UpdateDetails(
            "Meta",
            null,
            null,
            new DateTime(2026, 2, 14, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 2, 13, 0, 0, 0, DateTimeKind.Utc),
            MissionStatus.Active);

        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void Mission_UpdateDetails_WithNameLongerThan200_ShouldThrow()
    {
        var mission = Mission.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Meta",
            null,
            null,
            new DateTime(2026, 2, 12, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 2, 13, 0, 0, 0, DateTimeKind.Utc),
            MissionStatus.Active);

        var longName = new string('A', 201);
        var act = () => mission.UpdateDetails(
            longName,
            null,
            null,
            new DateTime(2026, 2, 12, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 2, 13, 0, 0, 0, DateTimeKind.Utc),
            MissionStatus.Active);

        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void MissionIndicator_ApplyTarget_WithInvalidRange_ShouldThrow()
    {
        var indicator = Indicator.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Indicador", IndicatorType.Quantitative);

        var act = () => indicator.ApplyTarget(
            IndicatorType.Quantitative,
            QuantitativeIndicatorType.KeepBetween,
            100m,
            100m,
            IndicatorUnit.Percentage,
            null);

        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void Checkin_Update_WithConfidenceOutOfRange_ShouldThrow()
    {
        var checkin = Checkin.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            10m,
            null,
            DateTime.UtcNow,
            null,
            3);

        var act = () => checkin.Update(10m, null, DateTime.UtcNow, null, 0);

        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void MissionIndicator_CreateCheckin_WithMissingQuantitativeValue_ShouldThrow()
    {
        var indicator = Indicator.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Indicador", IndicatorType.Quantitative);

        var act = () => indicator.CreateCheckin(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            DateTime.UtcNow,
            null,
            3);

        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void MissionIndicator_UpdateCheckin_WithMissingQualitativeText_ShouldThrow()
    {
        var indicator = Indicator.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Indicador", IndicatorType.Qualitative);
        var checkin = Checkin.Create(
            Guid.NewGuid(),
            indicator.OrganizationId,
            indicator.Id,
            Guid.NewGuid(),
            null,
            "Texto",
            DateTime.UtcNow,
            null,
            3);

        var act = () => indicator.UpdateCheckin(checkin, null, "   ", DateTime.UtcNow, null, 3);

        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void TemplateIndicator_Create_WithQuantitativeTypeMissing_ShouldThrow()
    {
        var act = () => TemplateIndicator.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Receita",
            IndicatorType.Quantitative,
            0,
            null,
            null,
            0m,
            100m,
            IndicatorUnit.Percentage,
            null);

        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void Template_ReplaceIndicators_ShouldSetTemplateAndOrganizationIds()
    {
        var template = Template.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Template",
            null,
            null,
            null);

        template.ReplaceIndicators(
        [
            new TemplateIndicatorDraft(
                "Qualidade",
                IndicatorType.Qualitative,
                0,
                null,
                null,
                null,
                null,
                null,
                "Meta textual")
        ]);

        template.Indicators.Should().ContainSingle();
        template.Indicators.First().TemplateId.Should().Be(template.Id);
        template.Indicators.First().OrganizationId.Should().Be(template.OrganizationId);
    }

    [Fact]
    public void Mission_Create_WithEmptyOrganization_ShouldThrow()
    {
        var act = () => Mission.Create(Guid.NewGuid(), Guid.Empty, "Meta", null, null, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), MissionStatus.Planned);

        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void Mission_Create_WithEmptyName_ShouldThrow()
    {
        var act = () => Mission.Create(Guid.NewGuid(), Guid.NewGuid(), "  ", null, null, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), MissionStatus.Planned);

        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void Mission_Create_WithNameLongerThan200_ShouldThrow()
    {
        var longName = new string('A', 201);
        var act = () => Mission.Create(Guid.NewGuid(), Guid.NewGuid(), longName, null, null, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), MissionStatus.Planned);

        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void Mission_Create_WithValidData_ShouldSucceed()
    {
        var id = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var parent = Mission.Create(Guid.NewGuid(), orgId, "Meta pai", null, null, DateTime.UtcNow, DateTime.UtcNow.AddDays(30), MissionStatus.Planned);

        var mission = Mission.Create(id, orgId, "Meta", "Descrição", null, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), MissionStatus.Planned, parent.Id);

        mission.Id.Should().Be(id);
        mission.OrganizationId.Should().Be(orgId);
        mission.ParentId.Should().Be(parent.Id);
        mission.Name.Should().Be("Meta");
        mission.Description.Should().Be("Descrição");
    }

    [Fact]
    public void Mission_UpdateDetails_WithEmptyName_ShouldThrow()
    {
        var mission = Mission.Create(Guid.NewGuid(), Guid.NewGuid(), "Meta", null, null, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), MissionStatus.Planned);

        var act = () => mission.UpdateDetails("", null, null, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), MissionStatus.Planned);

        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void Mission_UpdateDetails_TrimsDescription()
    {
        var mission = Mission.Create(Guid.NewGuid(), Guid.NewGuid(), "Meta", null, null, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), MissionStatus.Planned);

        mission.UpdateDetails("Novo Nome", "  Descrição  ", null, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), MissionStatus.Planned);

        mission.Name.Should().Be("Novo Nome");
        mission.Description.Should().Be("Descrição");
    }

    [Fact]
    public void Mission_UpdateDetails_NullsEmptyDescription()
    {
        var mission = Mission.Create(Guid.NewGuid(), Guid.NewGuid(), "Meta", "Desc", null, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), MissionStatus.Planned);

        mission.UpdateDetails("Nome", "   ", null, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), MissionStatus.Planned);

        mission.Description.Should().BeNull();
    }

    [Fact]
    public void Mission_UpdateDetails_SetsDimension()
    {
        var mission = Mission.Create(Guid.NewGuid(), Guid.NewGuid(), "Meta", null, null, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), MissionStatus.Planned);
        var dimension = "Clientes";

        mission.UpdateDetails("Meta", null, dimension, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), MissionStatus.Planned);

        mission.Dimension.Should().Be(dimension);
    }

    [Fact]
    public void Notification_Create_WithEmptyTitle_ShouldThrow()
    {
        var act = () => Notification.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "  ",
            "Mensagem",
            NotificationType.MissionCreated,
            DateTime.UtcNow);

        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void EmployeeAccessLog_Create_WithEmptyEmployee_ShouldThrow()
    {
        var act = () => EmployeeAccessLog.Create(
            Guid.NewGuid(),
            Guid.Empty,
            Guid.NewGuid(),
            DateTime.UtcNow);

        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void Mission_Create_WithParentId_SetsParentId()
    {
        var parentId = Guid.NewGuid();

        var child = Mission.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Meta filha",
            null,
            null,
            new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc),
            MissionStatus.Planned,
            parentId);

        child.ParentId.Should().Be(parentId);
    }
}
