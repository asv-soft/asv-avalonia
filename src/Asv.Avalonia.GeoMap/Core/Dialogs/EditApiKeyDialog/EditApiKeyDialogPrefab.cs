using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public sealed class EditApiKeyDialogPayload
{
    public string? CurrentApiKey { get; init; }
}

public sealed class EditApiKeyDialogPrefab(INavigationService nav, ILoggerFactory loggerFactory)
    : IDialogPrefab<EditApiKeyDialogPayload, string?>
{
    public async Task<string?> ShowDialogAsync(EditApiKeyDialogPayload dialogPayload)
    {
        using var vm = new EditApiKeyDialogViewModel(loggerFactory, dialogPayload.CurrentApiKey);

        var dialogContent = new ContentDialog(vm, nav)
        {
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
