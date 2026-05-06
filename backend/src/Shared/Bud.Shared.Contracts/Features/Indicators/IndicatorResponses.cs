using Bud.Shared.Kernel;

namespace Bud.Shared.Contracts.Features.Indicators;

public sealed class IndicatorResponse
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid MissionId { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid? ParentKrId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IndicatorMeasurementMode MeasurementMode { get; set; }
    public IndicatorGoalType GoalType { get; set; }
    public decimal StartValue { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal? TargetValue { get; set; }
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
    public Guid? LinkedMissionId { get; set; }
    public Guid? LinkedSurveyId { get; set; }
    public IndicatorExternalSource? ExternalSource { get; set; }
    public string? ExternalConfig { get; set; }
    public string SortOrder { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<CheckinResponse> Checkins { get; set; } = [];
}

public sealed class CheckinResponse
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid IndicatorId { get; set; }
    public Guid EmployeeId { get; set; }
    public decimal Value { get; set; }
    public decimal PreviousValue { get; set; }
    public CheckinConfidence? Confidence { get; set; }
    public string? Note { get; set; }
    public Guid[] Mentions { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public EmployeeMembershipResponse? Employee { get; set; }
}

public sealed class IndicatorProgressResponse
{
    public Guid IndicatorId { get; set; }
    public decimal Progress { get; set; }
    public int Confidence { get; set; }
    public bool HasCheckins { get; set; }
    public bool IsOutdated { get; set; }
    public string? LastCheckinEmployeeName { get; set; }
}
