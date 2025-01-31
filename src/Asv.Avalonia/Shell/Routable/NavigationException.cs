namespace Asv.Avalonia;

public class NavigationException : Exception
{
    public NavigationException()
    {
    }

    public NavigationException(string message)
        : base(message)
    {
    }

    public NavigationException(string message, Exception inner) 
        : base(message, inner)
    {
    }
}