using Bud.Application.Common;

namespace Bud.Application.Features.Missions;

internal static class MissionDateRangePolicy
{
    public static Result<T>? ValidateChildWindow<T>(
        DateTime childStartDate,
        DateTime childEndDate,
        DateTime parentStartDate,
        DateTime parentEndDate)
    {
        if (childStartDate < parentStartDate)
        {
            return Result<T>.Failure(
                $"A data de início da meta não pode ser anterior à do pai ({parentStartDate:dd/MM/yyyy}).",
                ErrorType.Validation);
        }

        if (childEndDate > parentEndDate)
        {
            return Result<T>.Failure(
                $"A data de término da meta não pode ser posterior à do pai ({parentEndDate:dd/MM/yyyy}).",
                ErrorType.Validation);
        }

        return null;
    }
}
