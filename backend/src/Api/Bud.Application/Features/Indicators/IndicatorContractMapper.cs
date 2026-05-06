using Bud.Application.Features.Employees;

namespace Bud.Application.Features.Indicators;

public static class IndicatorContractMapper
{
    public static IndicatorResponse ToResponse(this Indicator source)
    {
        return new IndicatorResponse
        {
            Id = source.Id,
            OrganizationId = source.OrganizationId,
            MissionId = source.MissionId,
            EmployeeId = source.EmployeeId,
            ParentKrId = source.ParentKrId,
            Title = source.Title,
            Description = source.Description,
            MeasurementMode = source.MeasurementMode,
            GoalType = source.GoalType,
            StartValue = source.StartValue,
            CurrentValue = source.CurrentValue,
            TargetValue = source.TargetValue,
            LowThreshold = source.LowThreshold,
            HighThreshold = source.HighThreshold,
            Unit = source.Unit,
            UnitLabel = source.UnitLabel,
            ExpectedValue = source.ExpectedValue,
            Status = source.Status,
            Progress = source.Progress,
            PeriodLabel = source.PeriodLabel,
            PeriodStart = source.PeriodStart,
            PeriodEnd = source.PeriodEnd,
            LinkedMissionId = source.LinkedMissionId,
            LinkedSurveyId = source.LinkedSurveyId,
            ExternalSource = source.ExternalSource,
            ExternalConfig = source.ExternalConfig,
            SortOrder = source.SortOrder,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt,
            Checkins = source.Checkins.Select(c => c.ToResponse()).ToList()
        };
    }

    public static CheckinResponse ToResponse(this Checkin source)
    {
        return new CheckinResponse
        {
            Id = source.Id,
            OrganizationId = source.OrganizationId,
            IndicatorId = source.IndicatorId,
            EmployeeId = source.EmployeeId,
            Value = source.Value,
            PreviousValue = source.PreviousValue,
            Confidence = source.Confidence,
            Note = source.Note,
            Mentions = source.Mentions,
            CreatedAt = source.CreatedAt,
            Employee = source.Employee?.ToEmployeeMembershipResponse()
        };
    }
}
