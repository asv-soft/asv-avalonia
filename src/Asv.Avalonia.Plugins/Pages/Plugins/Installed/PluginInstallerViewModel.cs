using Asv.Cfg;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Plugins;

public class PluginInstallerViewModelConfig
{
    public string NugetPackageFilePath { get; set; }
}

public class PluginInstallerViewModel : DialogViewModelBase
{
    public const string ViewModelId = $"{BaseId}.plugins.installed.installer";
    private readonly IPluginManager _manager;

    public PluginInstallerViewModel()
        : base(DesignTime.Id, NullLayoutService.Instance, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public PluginInstallerViewModel(
        IConfiguration cfg,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        IPluginManager manager
    )
        : base(ViewModelId, layoutService, loggerFactory)
    {
        _manager = manager;
        var config = cfg.Get<PluginInstallerViewModelConfig>();
        NugetPackageFilePath = new BindableReactiveProperty<string>(config.NugetPackageFilePath);

        _sub1 = NugetPackageFilePath.EnableValidationRoutable(
            value =>
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return new ValidationResult
                    {
                        IsSuccess = false,
                        ValidationException = new ValidationException(
                            "Nuget package file path cannot be empty"
                        ),
                    };
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

    public BindableReactiveProperty<string> NugetPackageFilePath { get; }

    internal async Task InstallPluginAsync(IProgress<double> progress, CancellationToken cancel)
    {
        await _manager.InstallManually(
            NugetPackageFilePath.Value,
            new Progress<ProgressMessage>(m => progress.Report(m.Progress)),
            cancel
        );
        Logger.LogInformation("Plugin installed successfully");
    }

    public override void ApplyDialog(ContentDialog dialog)
    {
        ArgumentNullException.ThrowIfNull(dialog);

        _sub2 = IsValid.Subscribe(isValid =>
        {
            dialog.IsPrimaryButtonEnabled = isValid;
        });
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
