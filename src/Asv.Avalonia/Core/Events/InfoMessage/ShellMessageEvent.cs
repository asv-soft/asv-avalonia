using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using R3;

namespace Asv.Avalonia.InfoMessage;

public class ShellMessageEvent(IViewModel source, ShellMessage message)
    : AsyncRoutedEvent<IViewModel>(source, RoutingStrategy.Bubble)
{
    public ShellMessage Message => message;
}

public static class ShellMessageEventMixin
{
    public static ValueTask RiseShellInfoMessage(
        this IViewModel source,
        ShellMessage message,
        CancellationToken cancel = default
    )
    {
        return source.Rise(new ShellMessageEvent(source, message), cancel);
    }

    public static ValueTask RiseShellErrorMessage(
        this IViewModel source,
        string title,
        string message,
        Exception exception,
        CancellationToken cancel = default
    )
    {
        return source.Rise(
            new ShellMessageEvent(
                source,
                new ShellMessage(title, message, ShellErrorState.Error, exception.ToString())
            ),
            cancel
        );
    }

    public static ValueTask RiseShellWarningMessage(
        this IViewModel source,
        string title,
        string message,
        Exception? exception = null,
        CancellationToken cancel = default
    )
    {
        return source.Rise(
            new ShellMessageEvent(
                source,
                new ShellMessage(title, message, ShellErrorState.Warning, exception?.ToString())
            ),
            cancel
        );
    }
}
