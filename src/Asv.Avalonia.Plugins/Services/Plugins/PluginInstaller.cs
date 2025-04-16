using Asv.Cfg;
using Asv.Common;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Plugins;

public class PluginInstaller(
    IConfiguration cfg,
    ILoggerFactory loggerFactory,
    IPluginManager manager,
    INavigationService navigationService
)
{
    public async Task ShowInstallDialog(IProgress<double> progress, CancellationToken cancel)
    {
        var log = loggerFactory.CreateLogger<PluginInstaller>();
        var dialog = new ContentDialog(navigationService)
        {
            Title = RS.PluginInstallerViewModel_InstallDialog_Title,
            CloseButtonText = RS.PluginInstallerViewModel_InstallDialog_SecondaryButtonText,
            PrimaryButtonText = RS.PluginInstallerViewModel_InstallDialog_PrimaryButtonText,
        };

        using var viewModel = new PluginInstallerViewModel(cfg, loggerFactory, manager);

        viewModel.ApplyDialog(dialog);

        dialog.Content = viewModel;
        var res = await dialog.ShowAsync();

        if (res == ContentDialogResult.Primary)
        {
            viewModel
                .InstallPluginAsync(progress, cancel)
                .SafeFireAndForget(ex =>
                    log.LogError(ex, "An error occurred while the plugin was being installed")
                );
        }
    }
}
