using System.ComponentModel.DataAnnotations.Schema;

namespace Bud.Domain.Indicators;

public sealed class Indicator : ITenantEntity, IAggregateRoot, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public Guid MissionId { get; set; }
    public Mission Mission { get; set; } = null!;

    public Guid? ParentKrId { get; set; }
    public Indicator? ParentKr { get; set; }
    public ICollection<Indicator> SubIndicators { get; set; } = [];

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public IndicatorMeasurementMode MeasurementMode { get; set; }

    public Guid? LinkedMissionId { get; set; }
    public Mission? LinkedMission { get; set; }

    public Guid? LinkedSurveyId { get; set; }

    public IndicatorExternalSource? ExternalSource { get; set; }
    public string? ExternalConfig { get; set; }

    public IndicatorGoalType GoalType { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal StartValue { get; set; }

    [NotMapped]
    public decimal CurrentValue => Checkins.OrderByDescending(c => c.CreatedAt).FirstOrDefault()?.Value ?? StartValue;
    public decimal? LowThreshold { get; set; }
    public decimal? HighThreshold { get; set; }
    public IndicatorUnit Unit { get; set; }
    public string UnitLabel { get; set; } = string.Empty;
    public decimal? ExpectedValue { get; set; }

    public IndicatorStatus Status { get; set; }
    public int Progress { get; set; }

    public string? PeriodLabel { get; set; }
    public DateOnly? PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }

    public string SortOrder { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public ICollection<Checkin> Checkins { get; set; } = [];

    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static Indicator Create(
        Guid id,
        Guid organizationId,
        Guid missionId,
        Guid employeeId,
        string title,
        IndicatorMeasurementMode measurementMode,
        IndicatorGoalType goalType,
        decimal startValue,
        decimal? targetValue,
        decimal? lowThreshold,
        decimal? highThreshold,
        IndicatorUnit unit,
        string unitLabel,
        string sortOrder,
        Guid? parentKrId = null)
    {
        if (organizationId == Guid.Empty)
        {
            throw new DomainInvariantException("Indicador deve pertencer a uma organização válida.");
        }

        if (missionId == Guid.Empty)
        {
            throw new DomainInvariantException("Indicador deve pertencer a uma meta válida.");
        }

        if (!EntityName.TryCreate(title, out var entityName))
        {
            throw new DomainInvariantException("O título do indicador é obrigatório e deve ter até 200 caracteres.");
        }

        return new Indicator
        {
            Id = id,
            OrganizationId = organizationId,
            MissionId = missionId,
            EmployeeId = employeeId,
            Title = entityName.Value,
            MeasurementMode = measurementMode,
            GoalType = goalType,
            StartValue = startValue,
            TargetValue = targetValue,
            Unit = unit,
            UnitLabel = unitLabel,
            SortOrder = sortOrder,
            ParentKrId = parentKrId,
            Status = IndicatorStatus.OnTrack,
            Progress = 0,
            LowThreshold = lowThreshold,
            HighThreshold = highThreshold,
        };
    }

    public void UpdateDetails(
        string title,
        string? description,
        Guid employeeId,
        IndicatorMeasurementMode measurementMode,
        IndicatorGoalType goalType,
        decimal startValue,
        decimal? targetValue,
        decimal? lowThreshold,
        decimal? highThreshold,
        IndicatorUnit unit,
        string unitLabel,
        decimal? expectedValue,
        string? periodLabel,
        DateOnly? periodStart,
        DateOnly? periodEnd,
        Guid? linkedMissionId,
        Guid? linkedSurveyId,
        IndicatorExternalSource? externalSource,
        string? externalConfig)
    {
        if (!EntityName.TryCreate(title, out var entityName))
        {
            throw new DomainInvariantException("O título do indicador é obrigatório e deve ter até 200 caracteres.");
        }

        Title = entityName.Value;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        EmployeeId = employeeId;
        MeasurementMode = measurementMode;
        GoalType = goalType;
        StartValue = startValue;
        TargetValue = targetValue;
        LowThreshold = lowThreshold;
        HighThreshold = highThreshold;
        Unit = unit;
        UnitLabel = unitLabel;
        ExpectedValue = expectedValue;
        PeriodLabel = string.IsNullOrWhiteSpace(periodLabel) ? null : periodLabel.Trim();
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        LinkedMissionId = linkedMissionId;
        LinkedSurveyId = linkedSurveyId;
        ExternalSource = externalSource;
        ExternalConfig = string.IsNullOrWhiteSpace(externalConfig) ? null : externalConfig.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public Checkin CreateCheckin(
        Guid checkinId,
        Guid employeeId,
        decimal value,
        DateTime checkinDate,
        string? note,
        int confidenceLevel)
    {
        var checkin = Checkin.Create(
            checkinId,
            OrganizationId,
            Id,
            employeeId,
            value,
            checkinDate,
            note,
            confidenceLevel);

        AddDomainEvent(new CheckinCreatedDomainEvent(checkin.Id, Id, OrganizationId, employeeId, Title));
        return checkin;
    }

    public void UpdateCheckin(Checkin checkin, decimal value, DateTime checkinDate, string? note, int confidenceLevel)
    {
        if (checkin.IndicatorId != Id)
        {
            throw new DomainInvariantException("Check-in não pertence ao indicador informado.");
        }

        checkin.Update(value, checkinDate, note, confidenceLevel);
    }



    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
