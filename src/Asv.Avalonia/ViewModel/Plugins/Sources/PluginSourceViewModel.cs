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
        PluginsSourcesViewModel pluginsSourcesViewModel
    )
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(pluginServerInfo);
        Id = pluginServerInfo.SourceUri;
        Name = pluginServerInfo.Name;
        SourceUri = pluginServerInfo.SourceUri;
        Remove = pluginsSourcesViewModel.Remove;
        Edit = pluginsSourcesViewModel.Edit;
        Model = pluginServerInfo;
    }

    public IPluginServerInfo Model { get; set; } = null!;
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string SourceUri { get; set; }
    public ReactiveCommand<PluginSourceViewModel> Edit { get; }
    public ReactiveCommand<PluginSourceViewModel> Remove { get; }
    public BindableReactiveProperty<bool> IsEnabled { get; set; }
}
