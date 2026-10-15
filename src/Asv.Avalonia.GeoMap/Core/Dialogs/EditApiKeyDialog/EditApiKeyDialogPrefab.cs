using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public sealed class EditApiKeyDialogPayload
{
    /// <summary>
    /// Gets or inits the optional icon displayed before the dialog title.
    /// </summary>
    public MaterialIconKind? Icon { get; init; } = MaterialIconKind.Key;

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

    public string? CurrentApiKey { get; init; }
}

public sealed class EditApiKeyDialogPrefab(ILoggerFactory loggerFactory)
    : IDialogPrefab<EditApiKeyDialogPayload, string?>
{
    public async Task<string?> ShowDialogAsync(EditApiKeyDialogPayload dialogPayload)
    {
        using var vm = new EditApiKeyDialogViewModel(loggerFactory, dialogPayload.CurrentApiKey);

        var dialogContent = new ContentDialog(vm)
        {
            SizeMode = dialogPayload.SizeMode,
            Icon = dialogPayload.Icon,
            IconColor = dialogPayload.IconColor,
            Status = dialogPayload.Status,
            Title = RS.EditApiKeyDialogPrefab_Content_Title,
            PrimaryButtonText = RS.EditApiKeyDialogPrefab_Content_PrimaryButton,
            SecondaryButtonText = Avalonia.RS.DialogButton_Cancel,
            DefaultButton = ContentDialogButton.Primary,
        };

        vm.ApplyDialog(dialogContent);

        var result = await dialogContent.ShowAsync();

        if (result is ContentDialogResult.Primary)
        {
            return string.IsNullOrWhiteSpace(vm.ApiKey.Value) ? null : vm.ApiKey.Value;
        }

        return dialogPayload.CurrentApiKey;
    }
}
