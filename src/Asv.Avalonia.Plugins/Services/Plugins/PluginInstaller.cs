using Asv.Cfg;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Plugins;

public class PluginInstaller(
    IConfiguration cfg,
    ILoggerFactory loggerFactory,
    IPluginManager manager,
    INavigationService navigationService
)
{
    public async Task ShowInstallDialog()
    {
        var dialog = new ContentDialog(navigationService)
        {
            Title = RS.PluginInstallerViewModel_InstallDialog_Title,
            CloseButtonText = RS.PluginInstallerViewModel_InstallDialog_SecondaryButtonText,
            PrimaryButtonText = RS.PluginInstallerViewModel_InstallDialog_PrimaryButtonText,
        };

        using var viewModel = new PluginInstallerViewModel(cfg, loggerFactory, manager);

        viewModel.ApplyDialog(dialog);

        dialog.Content = viewModel;
        await dialog.ShowAsync();
    }
}
