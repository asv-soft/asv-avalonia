using Avalonia.Threading;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public sealed class HotKeyCaptureDialogPayload
{
    /// <summary>
    /// Gets or inits the optional icon displayed before the dialog title.
    /// </summary>
    public MaterialIconKind? Icon { get; init; } = MaterialIconKind.Keyboard;

    /// <summary>
    /// Gets or inits the palette color of the dialog icon.
    /// </summary>
    public AsvColorKind IconColor { get; init; } = AsvColorKind.None;

    /// <summary>
    /// Gets or inits the dialog sizing mode.
    /// </summary>
    public DialogSizeMode SizeMode { get; init; } = DialogSizeMode.Compact;

    /// <summary>
    /// Gets or inits the dialog status that determines the frame color.
    /// </summary>
    public ShellErrorState Status { get; init; } = ShellErrorState.Normal;

    /// <summary>Gets the cancellation token for the dialog lifetime.</summary>
    public CancellationToken CancellationToken { get; init; }

    public required string Title { get; init; }

    public required string Message { get; init; }

    public global::Avalonia.Input.KeyGesture? CurrentHotKey { get; init; }
}

public sealed class HotKeyCaptureDialogPrefab(ILoggerFactory loggerFactory)
    : IDialogPrefab<HotKeyCaptureDialogPayload, global::Avalonia.Input.KeyGesture?>
{
    public async Task<global::Avalonia.Input.KeyGesture?> ShowDialogAsync(
        HotKeyCaptureDialogPayload dialogPayload
    )
    {
        dialogPayload.CancellationToken.ThrowIfCancellationRequested();
        using var vm = new DialogItemHotKeyCaptureViewModel(loggerFactory)
        {
            Message = dialogPayload.Message,
        };
        vm.HotKey.OnNext(dialogPayload.CurrentHotKey);

        var dialogContent = new ContentDialog(vm)
        {
            SizeMode = dialogPayload.SizeMode,
            Icon = dialogPayload.Icon,
            IconColor = dialogPayload.IconColor,
            Status = dialogPayload.Status,
            Title = dialogPayload.Title,
            PrimaryButtonText = RS.DialogButton_Save,
            SecondaryButtonText = RS.DialogButton_DontSave,
            DefaultButton = ContentDialogButton.Primary,
        };

        using var subscription = vm.HotKey.Subscribe(gesture =>
            dialogContent.IsPrimaryButtonEnabled =
                gesture is { Key: not global::Avalonia.Input.Key.None }
        );
        var showTask = dialogContent.ShowAsync();
        using var cancellation = dialogPayload.CancellationToken.Register(() =>
            Dispatcher.UIThread.Post(() => dialogContent.Hide())
        );
        var result = await showTask;
        dialogPayload.CancellationToken.ThrowIfCancellationRequested();

        if (result == ContentDialogResult.Primary)
        {
            return vm.HotKey.Value;
        }

        return null;
    }
}
