using Material.Icons;
using R3;

namespace Asv.Avalonia.InfoMessage;

public class ShellMessageEvent(IRoutable source, ShellMessage message)
    : AsyncRoutedEvent(source, RoutingStrategy.Bubble)
{
    public ShellMessage Message => message;
}

public static class ShellMessageEventMixin
{
    public static ValueTask RaiseShellInfoMessage(this IRoutable source, ShellMessage message)
    {
        return source.Rise(new ShellMessageEvent(source, message));
    }

    public static ValueTask RaiseShellErrorMessage(
        this IRoutable source,
        string title,
        string message,
        Exception exception
    )
    {
        return source.Rise(
            new ShellMessageEvent(
                source,
                new ShellMessage(title, message, ShellErrorState.Error, exception.ToString())
            )
        );
    }
}
