using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public sealed class HotKeyCaptureDialogPayload
{
    public required string Title { get; init; }

    public required string Message { get; init; }

    public global::Avalonia.Input.KeyGesture? CurrentHotKey { get; init; }
}

public sealed class HotKeyCaptureDialogPrefab(IShellHost shellHost, ILoggerFactory loggerFactory)
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
            Title = dialogPayload.Title,
            PrimaryButtonText = RS.DialogButton_Save,
            SecondaryButtonText = RS.DialogButton_DontSave,
            DefaultButton = ContentDialogButton.Primary,
        };

        var result = shellHost.TopLevel is { } topLevel
            ? await dialogContent.ShowAsync(topLevel)
            : await dialogContent.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            return vm.HotKey.Value;
        }

        return null;
    }
}
