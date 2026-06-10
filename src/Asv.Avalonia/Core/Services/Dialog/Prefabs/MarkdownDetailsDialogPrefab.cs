using Avalonia.Input.Platform;

namespace Asv.Avalonia;

/// <summary>
/// Payload for Markdown details dialog prefab.
/// </summary>
public sealed class MarkdownDetailsDialogPayload
{
    /// <summary>
    /// Gets or inits the title of the dialog.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets or inits the Markdown text displayed in the dialog.
    /// </summary>
    public required string MarkdownText { get; init; }

    /// <summary>
    /// Gets or inits the original text copied to clipboard.
    /// </summary>
    public required string OriginalText { get; init; }
}

/// <summary>
/// Dialog that shows Markdown details and can copy the original text.
/// </summary>
public sealed class MarkdownDetailsDialogPrefab(IShellHost shellHost)
    : IDialogPrefab<MarkdownDetailsDialogPayload, bool>
{
    public async Task<bool> ShowDialogAsync(MarkdownDetailsDialogPayload dialogPayload)
    {
        using var vm = new DialogItemMarkdownViewModel(dialogPayload.MarkdownText);

        var dialogContent = new ContentDialog(vm)
        {
            Title = dialogPayload.Title,
            PrimaryButtonText = RS.ShellMessage_CopyDetails,
            CloseButtonText = RS.ShellView_WindowControlButton_Close,
            DefaultButton = ContentDialogButton.Close,
        };

        var result = shellHost.TopLevel is { } topLevel
            ? await dialogContent.ShowAsync(topLevel)
            : await dialogContent.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            await CopyToClipboard(dialogPayload.OriginalText);
            return true;
        }

        return false;
    }

    private async Task CopyToClipboard(string text)
    {
        try
        {
            var clipboard = shellHost.TopLevel?.Clipboard;
            if (clipboard is null || string.IsNullOrWhiteSpace(text))
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
