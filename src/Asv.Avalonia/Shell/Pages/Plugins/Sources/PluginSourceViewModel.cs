using R3;

namespace Asv.Avalonia;

public class PluginSourceViewModel : DisposableViewModel
{
    public PluginSourceViewModel()
        : base(string.Empty)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public PluginSourceViewModel(
        string id,
        IPluginServerInfo pluginServerInfo,
        ILogService log,
        PluginsSourcesViewModel pluginsSourcesViewModel
    )
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(pluginServerInfo);
        Id = new BindableReactiveProperty<string>(pluginServerInfo.SourceUri);
        Name = new BindableReactiveProperty<string>(pluginServerInfo.Name);
        SourceUri = new BindableReactiveProperty<string>(pluginServerInfo.SourceUri);
        Model = pluginServerInfo;
        IsEnabled = Observable.Return(true);
        Edit = IsEnabled.ToReactiveCommand<PluginSourceViewModel>(pluginsSourcesViewModel.EditImpl);
        Remove = IsEnabled.ToReactiveCommand<PluginSourceViewModel>(
            pluginsSourcesViewModel.RemoveImpl
        );
        Remove.IgnoreOnErrorResume(ex =>
            log.Error(
                pluginsSourcesViewModel.Title.Value,
                RS.PluginsSourcesViewModel_PluginsSourcesViewModel_ErrorToRemove,
                ex
            )
        );
    }

    public IPluginServerInfo Model { get; set; } = null!;
    public BindableReactiveProperty<string> Id { get; set; } = null!;
    public BindableReactiveProperty<string> Name { get; set; } = null!;
    public BindableReactiveProperty<string> SourceUri { get; set; }
    public ReactiveCommand<PluginSourceViewModel> Edit { get; }
    public ReactiveCommand<PluginSourceViewModel> Remove { get; }
    public Observable<bool> IsEnabled { get; set; }
}
