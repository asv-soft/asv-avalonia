namespace Asv.Avalonia;

public static class ErrorHandler
{
    public static ErrorResponse HandleErrorResult(Exception error)
    {
        var errorName = error.GetType().Name;
        return Errors.TryGetValue(errorName, out var errorProblemDetailsBuilder)
            ? errorProblemDetailsBuilder(error)
            : DefaultErrorResponse;
    }

    public static void AddErrors(Dictionary<string, Func<Exception, ErrorResponse>> errors)
    {
        foreach (var kvp in errors)
        {
            Errors[kvp.Key] = kvp.Value;
        }
    }

    private static readonly Dictionary<string, Func<Exception, ErrorResponse>> Errors = new()
    {
        {
            nameof(NotEnoughPermissionsError),
            _ => new ErrorResponse // TODO: сделать просто строкой
            {
                Detail = "Недостаточно прав", // TODO: локализовать
            }
        },
    };

    private static readonly ErrorResponse DefaultErrorResponse = new()
    {
        Detail = "Неизвестная ошибка", // TODO: локализовать
    };
}
