
namespace Bud.Application.Common;

internal static class ProgressContractMapper
{
    public static MissionProgressResponse ToResponse(this MissionProgressSnapshot source)
    {
        return new MissionProgressResponse
        {
            MissionId = source.MissionId,
            OverallProgress = source.OverallProgress,
            ExpectedProgress = source.ExpectedProgress,
            AverageConfidence = source.AverageConfidence,
            TotalIndicators = source.TotalIndicators,
            IndicatorsWithCheckins = source.IndicatorsWithCheckins,
            OutdatedIndicators = source.OutdatedIndicators,
            DirectChildren = source.DirectChildren,
            DirectIndicators = source.DirectIndicators,
            TodoTasks = source.TodoTasks,
            DoingTasks = source.DoingTasks,
            LastCheckinDate = source.LastCheckinDate,
            DistinctEmployeeIds = source.DistinctEmployeeIds
        };
    }

    public static IndicatorProgressResponse ToResponse(this IndicatorProgressSnapshot source)
    {
        return new IndicatorProgressResponse
        {
            IndicatorId = source.IndicatorId,
            Progress = source.Progress,
            Confidence = source.Confidence,
            HasCheckins = source.HasCheckins,
            IsOutdated = source.IsOutdated,
            LastCheckinEmployeeName = source.LastCheckinEmployeeName
        };
    }
}
