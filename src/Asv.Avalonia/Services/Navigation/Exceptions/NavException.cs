namespace Asv.Avalonia;

public class NavException : Exception
{
    private static ValueTask<IRoutable>? _exception;

    public NavException() { }

    public NavException(string message)
        : base(message) { }

    public NavException(string message, Exception inner)
        : base(message, inner) { }

    public static ValueTask<IRoutable> AsyncEmptyPathException()
    {
        _exception ??= ValueTask.FromException<IRoutable>(
            new NavException("Navigation path is empty")
        );
        return _exception.Value;
    }
}
