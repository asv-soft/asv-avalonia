using Asv.Cfg;
using FluentAvalonia.UI.Controls;

namespace Asv.Avalonia;

public class PluginInstaller(IConfiguration cfg, ILogService log, IPluginManager manager)
{
    public async Task ShowInstallDialog(string id)
    {
        var dialog = new ContentDialog
        {
            Title = RS.PluginInstallerViewModel_InstallDialog_Title,
            CloseButtonText = RS.PluginInstallerViewModel_InstallDialog_SecondaryButtonText,
            PrimaryButtonText = RS.PluginInstallerViewModel_InstallDialog_PrimaryButtonText,
        };

        using var viewModel = new PluginInstallerViewModel(id, cfg, log, manager);

        viewModel.ApplyDialog(dialog);

        dialog.Content = viewModel;
        await dialog.ShowAsync();
    }
}
