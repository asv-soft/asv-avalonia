using Asv.Common;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Plugins;

public class InstalledPluginInfoViewModel : RoutableViewModel
{
    public const string ViewModelIdPart = "plugin.installed";

    private readonly IPluginManager _manager;
    private readonly ILocalPluginInfo? _pluginInfo;

    public InstalledPluginInfoViewModel()
        : this(NullLocalPluginInfo.Instance, NullPluginManager.Instance, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public InstalledPluginInfoViewModel(
        ILocalPluginInfo pluginInfo,
        IPluginManager manager,
        ILoggerFactory loggerFactory
    )
        : base(new NavigationId(ViewModelIdPart, pluginInfo.Id), loggerFactory)
    {
        _manager = manager;
        _pluginInfo = pluginInfo;

        PluginId = pluginInfo.Id;
        Name = pluginInfo.Title;
        Author = pluginInfo.Authors;
        Description = pluginInfo.Description;
        SourceName = pluginInfo.SourceUri;
        LocalVersion = $"{pluginInfo.Version} (API: {pluginInfo.ApiVersion})";
        Icon = pluginInfo.Icon;
        IsContainsIcon = Icon != null;
        LoadingError = pluginInfo.LoadingError;
        var isUninstalled = new ReactiveProperty<bool>(pluginInfo.IsUninstalled).DisposeItWith(
            Disposable
        );
        IsUninstalled = new HistoricalBoolProperty(
            nameof(IsUninstalled),
            isUninstalled,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        IsLoaded = pluginInfo.IsLoaded;
        IsVerified = pluginInfo.IsVerified;
        if (Author is not null)
        {
            IsVerified = Author.Contains("https://github.com/asv-soft");
        }

        Uninstall = new ReactiveCommand(_ => UninstallImpl()).DisposeItWith(Disposable);
        CancelUninstall = new ReactiveCommand(_ => CancelUninstallImpl()).DisposeItWith(Disposable);
    }

    public string PluginId { get; }
    public string? Author { get; }
    public string? Name { get; }
    public string? Description { get; }
    public string SourceName { get; }
    public string LocalVersion { get; }
    public bool IsContainsIcon { get; }
    public Bitmap? Icon { get; }
    public bool IsLoaded { get; }
    public string LoadingError { get; }
    public bool IsVerified
    {
        get;
        private init => SetField(ref field, value);
    }

    public HistoricalBoolProperty IsUninstalled { get; }
    public ReactiveCommand Uninstall { get; }
    public ReactiveCommand CancelUninstall { get; }

    private void CancelUninstallImpl()
    {
        if (_pluginInfo is null)
        {
            Logger.LogError("Plugin is not installed");
            return;
        }

        _manager.CancelUninstall(_pluginInfo);
        IsUninstalled.ViewValue.OnNext(false);
    }

    private void UninstallImpl()
    {
        if (_pluginInfo is null)
        {
            Logger.LogError("Plugin is not installed");
            return;
        }

        _manager.Uninstall(_pluginInfo);
        IsUninstalled.ViewValue.OnNext(true);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return IsUninstalled;
    }
}
