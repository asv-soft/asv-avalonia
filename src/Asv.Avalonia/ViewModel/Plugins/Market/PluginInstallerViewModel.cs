using Asv.Cfg;
using FluentAvalonia.UI.Controls;
using R3;

namespace Asv.Avalonia;

public class PluginInstallerViewModelConfig
{
    public string NugetPackageFilePath { get; set; }
}

public class PluginInstallerViewModel : DisposableViewModel
{
    private readonly ILogService _log;
    private readonly IPluginManager _manager;

    public PluginInstallerViewModel()
        : base(string.Empty) { }

    public PluginInstallerViewModel(
        string id,
        IConfiguration cfg,
        ILogService log,
        IPluginManager manager
    )
        : base(id)
    {
        _log = log;
        _manager = manager;
        var config = cfg.Get<PluginInstallerViewModelConfig>();
        NugetPackageFilePath = new BindableReactiveProperty<string>(config.NugetPackageFilePath);

        NugetPackageFilePath.Subscribe(_ =>
        {
            config.NugetPackageFilePath = NugetPackageFilePath.Value;
            cfg.Set(config);
        });
    }

    public BindableReactiveProperty<string> NugetPackageFilePath { get; set; }

    private async Task InstallPluginAsync(IProgress<double> progress, CancellationToken cancel)
    {
        try
        {
            await _manager.InstallManually(
                NugetPackageFilePath.Value,
                new Progress<ProgressMessage>(m => progress.Report(m.Progress)),
                cancel
            );
            _log.Info(
                nameof(PluginManager),
                RS.PluginInstallerViewModel_InstallPluginAsync_Success
            );
        }
        catch (Exception e)
        {
            _log.Error(nameof(PluginManager), e.Message);
        }
    }

    public void ApplyDialog(ContentDialog dialog)
    {
        ArgumentNullException.ThrowIfNull(dialog);

        dialog.PrimaryButtonCommand = new ReactiveCommand<IProgress<double>>(p =>
            Task.FromResult(InstallPluginAsync(p, CancellationToken.None))
        );
    }
}
