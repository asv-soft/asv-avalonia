namespace Asv.Avalonia;

public class NavException : Exception
{
    private static ValueTask<IViewModel>? _exception;

    public NavException() { }

    public NavException(string message)
        : base(message) { }

    public NavException(string message, Exception inner)
        : base(message, inner) { }

    public static void ThrowEmptyPathException()
    {
        throw new NavException("Navigation path is empty");
    }

    public static ValueTask<IViewModel> AsyncEmptyPathException()
    {
        _exception ??= ValueTask.FromException<IViewModel>(
            new NavException("Navigation path is empty")
        );
        return _exception.Value;
    }
}
