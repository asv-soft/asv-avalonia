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
    public static ValueTask RaiseShellInfoMessage(
        this IViewModel source,
        ShellMessage message,
        CancellationToken cancel
    )
    {
        return source.Rise(new ShellMessageEvent(source, message), cancel);
    }

    public static ValueTask RaiseShellErrorMessage(
        this IViewModel source,
        string title,
        string message,
        Exception exception,
        CancellationToken cancel
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
}
