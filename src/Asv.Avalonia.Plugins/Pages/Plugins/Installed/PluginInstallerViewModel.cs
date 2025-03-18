using Asv.Cfg;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Plugins;

public class PluginInstallerViewModelConfig
{
    public string NugetPackageFilePath { get; set; }
}

public class PluginInstallerViewModel : DialogViewModelBase
{
    public const string ViewModelId = "plugins.installed.installer.dialog";

    private readonly ILoggerFactory _loggerFactory;
    private readonly IPluginManager _manager;
    private readonly ILogger _logger;

    public PluginInstallerViewModel()
        : base(ViewModelId) { }

    public PluginInstallerViewModel(
        IConfiguration cfg,
        ILoggerFactory loggerFactory,
        IPluginManager manager
    )
        : base(ViewModelId)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<PluginInstallerViewModel>();

        _manager = manager;
        var config = cfg.Get<PluginInstallerViewModelConfig>();
        NugetPackageFilePath = new BindableReactiveProperty<string>(config.NugetPackageFilePath);

        _sub1 = NugetPackageFilePath.EnableValidation(
            value =>
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return ValueTask.FromResult<ValidationResult>(
                        new Exception(RS.SourceViewModel_SourceViewModel_NameIsRequired)
                    );
                }

                return ValidationResult.Success;
            },
            this,
            true
        );
        _sub3 = NugetPackageFilePath.Subscribe(value =>
        {
            config.NugetPackageFilePath = value;
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
            _logger.LogInformation(RS.PluginInstallerViewModel_InstallPluginAsync_Success);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
    }

    public void ApplyDialog(ContentDialog dialog)
    {
        ArgumentNullException.ThrowIfNull(dialog);

        _sub2 = IsValid.Subscribe(isValid =>
        {
            dialog.IsPrimaryButtonEnabled = isValid;
        });

        dialog.PrimaryButtonCommand = new ReactiveCommand<IProgress<double>>(
            async (p, ct) => await InstallPluginAsync(p, ct)
        );
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    #region Dispose

    private readonly IDisposable _sub1;
    private IDisposable _sub2;
    private readonly IDisposable _sub3;

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            _sub1.Dispose();
            _sub2.Dispose();
            _sub3.Dispose();
        }

        base.Dispose(isDisposing);
    }

    #endregion
}
