using System.Composition;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

#region Payload

public sealed class HotKeyCaptureDialogPayload
{
    public required string Title { get; init; }

    public required string Message { get; init; }

    public HotKeyInfo? CurrentHotKey { get; init; }
}

#endregion

#region Prefab

[ExportDialogPrefab]
[Shared]
[method: ImportingConstructor]
public sealed class HotKeyCaptureDialogPrefab(INavigationService nav, ILoggerFactory loggerFactory)
    : IDialogPrefab<HotKeyCaptureDialogPayload, HotKeyInfo?>
{
    public async Task<HotKeyInfo?> ShowDialogAsync(HotKeyCaptureDialogPayload dialogPayload)
    {
        using var vm = new DialogItemHotKeyCaptureViewModel(loggerFactory);
        vm.HotKey.OnNext(dialogPayload.CurrentHotKey);

        var dialogContent = new ContentDialog(vm, nav)
        {
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

        if (vm.Parent is not null)
        {
            await vm.Navigate(vm.Parent.Id);
        }
        return null;
    }
}

#endregion
