namespace Bud.Application.Common;

public static class ResultConversionExtensions
{
    public static Result<T> ToFailureResult<T>(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot convert a successful non-generic result into a failed generic result.");
        }

        return result.ErrorType switch
        {
            ErrorType.NotFound => Result<T>.NotFound(result.Error ?? "Recurso não encontrado."),
            ErrorType.Forbidden => Result<T>.Forbidden(result.Error ?? "Acesso negado."),
            ErrorType.Conflict => Result<T>.Failure(result.Error ?? "Conflito.", ErrorType.Conflict),
            _ => Result<T>.Failure(result.Error ?? "Falha de validação.", result.ErrorType)
        };
    }
}
