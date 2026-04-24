using Bud.Shared.Kernel;

namespace Bud.Shared.Contracts.Features.Indicators;

public sealed class IndicatorResponse
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid MissionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public IndicatorType Type { get; set; }
    public QuantitativeIndicatorType? QuantitativeType { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public IndicatorUnit? Unit { get; set; }
    public string? TargetText { get; set; }
    public List<CheckinResponse> Checkins { get; set; } = [];
}

public sealed class CheckinResponse
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid IndicatorId { get; set; }
    public Guid EmployeeId { get; set; }
    public decimal? Value { get; set; }
    public string? Text { get; set; }
    public DateTime CheckinDate { get; set; }
    public string? Note { get; set; }
    public int ConfidenceLevel { get; set; }
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
