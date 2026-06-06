using Asv.Common;
using Asv.Modeling;
using Avalonia.Input.Platform;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Avalonia.InfoMessage;

public class ShellMessageEvent(IViewModel source, ShellMessage message)
    : AsyncRoutedEvent<IViewModel>(source, RoutingStrategy.Bubble)
{
    public ShellMessage Message => message;
}

public static class ShellMessageEventMixin
{
    private static readonly CopyShellMessageTextCommand CopyMessageTextCommand = new();

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
        var description = exception.ToString();
        return source.Rise(
            new ShellMessageEvent(
                source,
                new ShellMessage(
                    title,
                    message,
                    ShellErrorState.Error,
                    description: description,
                    icon: MaterialIconKind.Alert,
                    command: CopyMessageTextCommand,
                    commandParam: CreateMessageDetails(title, message, description),
                    commandTitle: RS.ShellMessage_CopyDetails
                )
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
        var description = exception?.ToString();
        return source.Rise(
            new ShellMessageEvent(
                source,
                new ShellMessage(
                    title,
                    message,
                    ShellErrorState.Warning,
                    description: description,
                    icon: MaterialIconKind.AlertOutline,
                    command: CopyMessageTextCommand,
                    commandParam: CreateMessageDetails(title, message, description),
                    commandTitle: RS.ShellMessage_CopyDetails
                )
            ),
            cancel
        );
    }

    private static string CreateMessageDetails(string title, string message, string? description)
    {
        return string.IsNullOrWhiteSpace(description)
            ? $"{title}{Environment.NewLine}{message}"
            : $"{title}{Environment.NewLine}{message}{Environment.NewLine}{Environment.NewLine}{description}";
    }

    private sealed class CopyShellMessageTextCommand : System.Windows.Input.ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter)
        {
            return parameter is string text && !string.IsNullOrWhiteSpace(text);
        }

        public void Execute(object? parameter)
        {
            if (parameter is not string text || string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            CopyToClipboard(text).SafeFireAndForget();
        }

        private static async Task CopyToClipboard(string text)
        {
            try
            {
                var clipboard = AppHost
                    .Instance.Services.GetService<IShellHost>()
                    ?.TopLevel?.Clipboard;
                if (clipboard is null)
                {
                    return;
                }

                await clipboard.SetTextAsync(text);
            }
            catch (ObjectDisposedException)
            {
                // Ignore late copy requests while the application is shutting down.
            }
            catch (InvalidOperationException)
            {
                // The application host can be unavailable in design-time or test contexts.
            }
        }
    }
}
