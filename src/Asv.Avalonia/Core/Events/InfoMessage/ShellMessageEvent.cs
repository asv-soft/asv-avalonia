using System.Text;
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
    private static readonly ReactiveCommand<MarkdownDetailsDialogPayload> ShowMessageDetailsCommand =
        new((payload, cancel) => ShowDetails(payload, cancel), AwaitOperation.Drop);

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
                    command: ShowMessageDetailsCommand,
                    commandParam: CreateMessageDetails(title, message, exception),
                    commandTitle: RS.ShellMessage_ShowDetails
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
                    command: ShowMessageDetailsCommand,
                    commandParam: CreateMessageDetails(title, message, exception),
                    commandTitle: RS.ShellMessage_ShowDetails
                )
            ),
            cancel
        );
    }

    private static MarkdownDetailsDialogPayload CreateMessageDetails(
        string title,
        string message,
        Exception? exception
    )
    {
        var description = exception?.ToString();
        return new MarkdownDetailsDialogPayload
        {
            Title = title,
            MarkdownText = exception is null
                ? CreateMessageMarkdownText(message, description)
                : CreateExceptionMarkdownText(message, exception),
            OriginalText = CreateOriginalText(title, message, description),
        };
    }

    private static async ValueTask ShowDetails(
        MarkdownDetailsDialogPayload payload,
        CancellationToken cancel
    )
    {
        var copyFallback = false;

        try
        {
            cancel.ThrowIfCancellationRequested();

            var dialogService = AppHost.Instance.Services.GetService<IDialogService>();
            if (
                dialogService?.TryGetDialogPrefab<MarkdownDetailsDialogPrefab>(out var dialog)
                    == true
                && dialog is not null
            )
            {
                await dialog.ShowDialogAsync(payload);
                return;
            }

            copyFallback = true;
        }
        catch (OperationCanceledException) when (cancel.IsCancellationRequested)
        {
            return;
        }
        catch (ObjectDisposedException)
        {
            // Ignore late requests while the application is shutting down.
            return;
        }
        catch (InvalidOperationException)
        {
            copyFallback = true;
        }
        catch (Exception)
        {
            copyFallback = true;
        }

        if (copyFallback)
        {
            await CopyToClipboard(payload.OriginalText);
        }
    }

    private static async ValueTask CopyToClipboard(string text)
    {
        try
        {
            var clipboard = AppHost.Instance.Services.GetService<IShellHost>()?.TopLevel?.Clipboard;
            if (clipboard is null || string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            await clipboard.SetTextAsync(text);
        }
        catch (ObjectDisposedException)
        {
            // Ignore late requests while the application is shutting down.
        }
        catch (InvalidOperationException)
        {
            // The application host can be unavailable in design-time or test contexts.
        }
    }

    private static string CreateOriginalText(string title, string message, string? description)
    {
        return string.IsNullOrWhiteSpace(description)
            ? $"{title}{Environment.NewLine}{message}"
            : $"{title}{Environment.NewLine}{message}{Environment.NewLine}{Environment.NewLine}{description}";
    }

    private static string CreateMessageMarkdownText(string message, string? description)
    {
        var builder = new StringBuilder();
        AppendEscapedParagraph(builder, message);

        if (!string.IsNullOrWhiteSpace(description))
        {
            foreach (var line in NormalizeLines(description))
            {
                AppendEscapedParagraph(builder, line);
            }
        }

        return builder.ToString().TrimEnd();
    }

    private static string CreateExceptionMarkdownText(string shellMessage, Exception exception)
    {
        var builder = new StringBuilder();
        builder.Append("# [icon=Alert;color=Error;] ");
        builder.AppendLine(EscapeMarkdown(exception.GetType().Name));
        builder.AppendLine();

        AppendColoredParagraph(builder, "Error", exception.Message);

        builder.AppendLine("## [icon=Information;color=Info5;] Summary");
        AppendPlainBullet(builder, "Shell message", shellMessage);
        AppendPlainBullet(builder, "Type", exception.GetType().FullName);
        AppendPlainBullet(builder, "Message", exception.Message);
        AppendPlainMetadataBullets(builder, exception);

        if (exception is AggregateException aggregateException)
        {
            AppendPlainBullet(
                builder,
                "Aggregate exceptions",
                aggregateException.InnerExceptions.Count.ToString()
            );
        }

        builder.AppendLine();
        AppendStackTraceSection(builder, exception, "##");
        AppendInnerExceptions(builder, exception, 0);

        return builder.ToString().TrimEnd();
    }

    private static void AppendEscapedParagraph(StringBuilder builder, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            builder.AppendLine();
            return;
        }

        builder.AppendLine(EscapeMarkdown(text));
        builder.AppendLine();
    }

    private static void AppendColoredParagraph(StringBuilder builder, string color, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        builder.Append("[color=");
        builder.Append(color);
        builder.Append(";]");
        builder.Append(EscapeMarkdown(text));
        builder.AppendLine("[/color]");
        builder.AppendLine();
    }

    private static void AppendBullet(
        StringBuilder builder,
        string icon,
        string color,
        string label,
        string? value
    )
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        builder.Append("- [icon=");
        builder.Append(icon);
        builder.Append(";color=");
        builder.Append(color);
        builder.Append(";] [color=");
        builder.Append(color);
        builder.Append(";]");
        builder.Append(EscapeMarkdown(label));
        builder.Append(":[/color] ");
        builder.AppendLine(EscapeMarkdown(value));
    }

    private static void AppendPlainBullet(StringBuilder builder, string label, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        builder.Append("- **");
        builder.Append(EscapeMarkdown(label));
        builder.Append(":** ");
        builder.AppendLine(EscapeMarkdown(value));
    }

    private static void AppendMetadataBullets(StringBuilder builder, Exception exception)
    {
        AppendBullet(builder, "Information", "Info5", "HResult", $"0x{exception.HResult:X8}");
        AppendBullet(builder, "Information", "Info5", "Source", exception.Source);

        if (exception.TargetSite is not null)
        {
            AppendBullet(
                builder,
                "Information",
                "Info5",
                "Target",
                $"{exception.TargetSite.DeclaringType?.FullName}.{exception.TargetSite.Name}"
            );
        }

        if (exception.Data?.Count > 0)
        {
            foreach (var key in exception.Data.Keys)
            {
                AppendBullet(
                    builder,
                    "Information",
                    "Info5",
                    $"Data {key}",
                    exception.Data[key]?.ToString()
                );
            }
        }
    }

    private static void AppendPlainMetadataBullets(StringBuilder builder, Exception exception)
    {
        AppendPlainBullet(builder, "HResult", $"0x{exception.HResult:X8}");
        AppendPlainBullet(builder, "Source", exception.Source);

        if (exception.TargetSite is not null)
        {
            AppendPlainBullet(
                builder,
                "Target",
                $"{exception.TargetSite.DeclaringType?.FullName}.{exception.TargetSite.Name}"
            );
        }

        if (exception.Data?.Count > 0)
        {
            foreach (var key in exception.Data.Keys)
            {
                AppendPlainBullet(builder, $"Data {key}", exception.Data[key]?.ToString());
            }
        }
    }

    private static void AppendStackTraceSection(
        StringBuilder builder,
        Exception exception,
        string heading
    )
    {
        var frames = ReadStackFrames(exception.StackTrace);
        if (frames.Count == 0)
        {
            return;
        }

        builder.Append(heading);
        builder.AppendLine(" [icon=CodeBraces;color=Info12;] Stack trace");
        for (var i = 0; i < frames.Count; i++)
        {
            builder.Append(i + 1);
            builder.Append(". ");
            builder.AppendLine(EscapeMarkdown(frames[i]));
        }

        builder.AppendLine();
    }

    private static void AppendInnerExceptions(StringBuilder builder, Exception exception, int depth)
    {
        const int maxDepth = 8;
        if (depth >= maxDepth)
        {
            builder.AppendLine(
                "### [icon=AlertOutline;color=Warning;] Inner exception depth limit reached"
            );
            builder.AppendLine();
            return;
        }

        if (exception is AggregateException aggregateException)
        {
            for (var i = 0; i < aggregateException.InnerExceptions.Count; i++)
            {
                AppendNestedException(
                    builder,
                    aggregateException.InnerExceptions[i],
                    depth,
                    $"Aggregate inner #{i + 1}"
                );
            }

            return;
        }

        if (exception.InnerException is not null)
        {
            AppendNestedException(builder, exception.InnerException, depth, "Inner exception");
        }
    }

    private static void AppendNestedException(
        StringBuilder builder,
        Exception exception,
        int depth,
        string label
    )
    {
        builder.AppendLine(
            $"### [icon=AlertCircle;color=Warning;] {EscapeMarkdown(label)}: {EscapeMarkdown(exception.GetType().Name)}"
        );
        AppendColoredParagraph(builder, "Warning", exception.Message);
        AppendBullet(builder, "Bug", "Warning", "Type", exception.GetType().FullName);
        AppendMetadataBullets(builder, exception);
        builder.AppendLine();
        AppendStackTraceSection(builder, exception, "###");
        AppendInnerExceptions(builder, exception, depth + 1);
    }

    private static IEnumerable<string> NormalizeLines(string text)
    {
        return text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
    }

    private static IReadOnlyList<string> ReadStackFrames(string? stackTrace)
    {
        if (string.IsNullOrWhiteSpace(stackTrace))
        {
            return [];
        }

        var frames = new List<string>();
        using var reader = new StringReader(stackTrace);
        while (reader.ReadLine() is { } line)
        {
            var frame = line.Trim();
            if (frame.StartsWith("at ", StringComparison.Ordinal))
            {
                frame = frame[3..];
            }

            if (!string.IsNullOrWhiteSpace(frame))
            {
                frames.Add(frame);
            }
        }

        return frames;
    }

    private static string EscapeMarkdown(string text)
    {
        return text.Replace(@"\", @"\\")
            .Replace("[", @"\[")
            .Replace("]", @"\]")
            .Replace("*", @"\*");
    }
}
