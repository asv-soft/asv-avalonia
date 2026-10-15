using Material.Icons;
using Microsoft.Extensions.Logging;

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
        using var vm = new DialogItemHotKeyCaptureViewModel(loggerFactory);
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

        var result = await dialogContent.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            return vm.HotKey.Value;
        }

        return null;
    }
}
