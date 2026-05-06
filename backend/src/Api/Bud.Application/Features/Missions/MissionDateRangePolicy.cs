using Bud.Application.Common;

namespace Bud.Application.Features.Missions;

internal static class MissionDateRangePolicy
{
    public static Result<T>? ValidateChildDueDate<T>(DateTime? childDueDate, DateTime? parentDueDate)
    {
        if (childDueDate is null || parentDueDate is null)
        {
            return null;
        }

        if (childDueDate.Value > parentDueDate.Value)
        {
            return Result<T>.Failure(
                $"A data de entrega da meta não pode ser posterior à do pai ({parentDueDate.Value:dd/MM/yyyy}).",
                ErrorType.Validation);
        }

        return null;
    }
}
