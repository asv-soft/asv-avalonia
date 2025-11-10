using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Plugins;

public class InstalledPluginInfoViewModel : RoutableViewModel
{
    private readonly IPluginManager _manager;
    private readonly ILocalPluginInfo? _pluginInfo;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public InstalledPluginInfoViewModel()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        : this(
            NavigationId.Empty,
            NullPluginManager.Instance,
            NullLocalPluginInfo.Instance,
            DesignTime.LoggerFactory
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        Uninstall = new ReactiveCommand(_ => { });
        CancelUninstall = new ReactiveCommand(_ => { });
    }

    public InstalledPluginInfoViewModel(
        NavigationId id,
        IPluginManager manager,
        ILocalPluginInfo pluginInfo,
        ILoggerFactory loggerFactory
    )
        : base(id, loggerFactory)
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
        LoadingError = new BindableReactiveProperty<string>(pluginInfo.LoadingError);
        IsUninstalled = new BindableReactiveProperty<bool>(pluginInfo.IsUninstalled);
        IsLoaded = new BindableReactiveProperty<bool>(pluginInfo.IsLoaded);
        IsVerified = new BindableReactiveProperty<bool>(pluginInfo.IsVerified);
        if (Author != null)
        {
            IsVerified.OnNext(Author.Contains("https://github.com/asv-soft"));
        }

        Uninstall = new ReactiveCommand(_ => UninstallImpl());
        CancelUninstall = new ReactiveCommand(_ => CancelUninstallImpl());
    }

    public string PluginId { get; set; }
    public string? Author { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string SourceName { get; set; }
    public string LocalVersion { get; set; }
    public bool IsContainsIcon { get; set; }
    public Bitmap? Icon { get; set; }
    public BindableReactiveProperty<string> LoadingError { get; set; }
    public BindableReactiveProperty<bool> IsLoaded { get; set; }
    public BindableReactiveProperty<bool> IsUninstalled { get; set; }
    public BindableReactiveProperty<bool> IsVerified { get; set; }

    public ReactiveCommand Uninstall { get; set; }
    public ReactiveCommand CancelUninstall { get; set; }

    private void CancelUninstallImpl()
    {
        if (_pluginInfo == null)
        {
            Logger.LogError("Plugin is not installed");
            return;
        }

        _manager.CancelUninstall(_pluginInfo);
        IsUninstalled.OnNext(false);
    }

    private void UninstallImpl()
    {
        if (_pluginInfo == null)
        {
            Logger.LogError("Plugin is not installed");
            return;
        }

        _manager.Uninstall(_pluginInfo);
        IsUninstalled.OnNext(true);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
